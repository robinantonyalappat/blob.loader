
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using blob.loader.Interfaces;

//using System.Net.Http.Headers;
using Azure.Core.Pipeline;
using blob.loader.Unused.Helpers;

namespace blob.loader.Services;

public class StreamFileUploadService : IStreamFileUploadService
{
    private readonly long _fileSizeLimit;
    private readonly string[] _permittedExtensions = { ".*" };


    private BlobClient GetBlobClient(string blobName)
    {
        var accountName = "vs2rgstorageaccount";
        //NOTE::::::::add access key here from the storage:::::::::::::::
        //https://portal.azure.com/#@ashleyalexlive.onmicrosoft.com/resource/subscriptions/53930ecc-65bc-4f45-87e7-942a7d4e9c9d/resourceGroups/vs2-rg-blob-loader/providers/Microsoft.Storage/storageAccounts/vs2rgstorageaccount/keys
        var accountAccessKey = "";
        var storageAccountUri = "https://" + accountName + ".blob.core.windows.net";

        var blobClientOptions = new BlobClientOptions
        {
            //Transport = new HttpClientTransport(new HttpClient { Timeout = TimeSpan.FromSeconds(102) })
            Transport = new HttpClientTransport(new HttpClient { Timeout = Timeout.InfiniteTimeSpan }),
            Retry = { NetworkTimeout = Timeout.InfiniteTimeSpan }
        };
        var credential = new StorageSharedKeyCredential(accountName, accountAccessKey);
        var storageAccountBlobServiceClient = new BlobServiceClient(new Uri(storageAccountUri), credential, options: blobClientOptions);

        var blobContainerName = "upload-files";
        var blobContainerClient = storageAccountBlobServiceClient.GetBlobContainerClient(blobContainerName);

        var blobClient = blobContainerClient.GetBlobClient(blobName);

        return blobClient;
    }

    /// <summary>
    /// NOTE::::::::add access key here from the storage in the GetBlobClient method:::::::::::::::
    /// https://portal.azure.com/#@ashleyalexlive.onmicrosoft.com/resource/subscriptions/53930ecc-65bc-4f45-87e7-942a7d4e9c9d/resourceGroups/vs2-rg-blob-loader/providers/Microsoft.Storage/storageAccounts/vs2rgstorageaccount/keys
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="section"></param>
    /// <returns></returns>
    public async Task<bool> UploadFile(MultipartReader reader, MultipartSection? section)
    {
        while (section != null)
        {
            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(
                section.ContentDisposition, out var contentDisposition
            );

            if (!hasContentDispositionHeader)
            {
                section = await reader.ReadNextSectionAsync();
                continue;
            }

            if (!MultipartRequestHelper
                .HasFileContentDisposition(contentDisposition))
            {
                return false;
            }

            if (contentDisposition.DispositionType.Equals("form-data") &&
                (!string.IsNullOrEmpty(contentDisposition.FileName.Value) ||
                !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value)))
            {
                var fileName = contentDisposition.FileName.Value;

                var blobClient = GetBlobClient(contentDisposition.FileName.Value);

                Azure.Response<BlobContentInfo> uploadResponse;

                ////1. Upload data from the local file
                //uploadResponse = await blobClient.UploadAsync(fileName, true);

                //2. 
                //var fullFilePath = Path.Combine(Environment.CurrentDirectory, contentDisposition.FileName.Value);
                //await blobClient.UploadAsync(fullFilePath, true);

                //3. æ
                byte[] fileArray;
                using (var memoryStream = new MemoryStream())
                {
                    await section.Body.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    uploadResponse = await blobClient.UploadAsync(memoryStream, true);
                }   
                
                
                //using (var ms = new MemoryStream(fileArray))
                //{
                //    Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

                //    uploadResponse = await blobClient.UploadAsync(ms, true);
                //}

                if (uploadResponse.GetRawResponse().Status == 201) //Success and Created
                {
                    return true;
                }
            }
        }
        return true;
    }

    ///// <summary>
    ///// NOTE::::::::add access key here from the storage in the GetBlobClient method:::::::::::::::
    ///// https://portal.azure.com/#@ashleyalexlive.onmicrosoft.com/resource/subscriptions/53930ecc-65bc-4f45-87e7-942a7d4e9c9d/resourceGroups/vs2-rg-blob-loader/providers/Microsoft.Storage/storageAccounts/vs2rgstorageaccount/keys
    ///// </summary>
    ///// <param name="reader"></param>
    ///// <param name="section"></param>
    ///// <returns></returns>
    //public async Task<bool> UploadFile(MultipartReader reader, MultipartSection? section)
    //{
    //    while (section != null)
    //    {
    //        var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(
    //            section.ContentDisposition, out var contentDisposition
    //        );

    //        if (!hasContentDispositionHeader)
    //        {
    //            section = await reader.ReadNextSectionAsync();
    //            continue;
    //        }

    //        if (!MultipartRequestHelper
    //            .HasFileContentDisposition(contentDisposition))
    //        {
    //            return false;
    //        }

    //        var streamedFileContent = await FileHelpers.ProcessStreamedFile(
    //            section, contentDisposition, _permittedExtensions, _fileSizeLimit);

    //        try
    //        {
    //            var blobClient = GetBlobClient(contentDisposition.FileName.Value);

    //            using (var ms = new MemoryStream(streamedFileContent))
    //            {
    //                Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

    //                var uploadResponse = await blobClient.UploadAsync(ms, true);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            throw;
    //        }
    //    }
    //    return true;
    //}
}