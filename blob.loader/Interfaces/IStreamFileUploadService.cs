using Microsoft.AspNetCore.WebUtilities;

namespace blob.loader.Interfaces;

public interface IStreamFileUploadService
{
    Task<bool> UploadFile(MultipartReader reader, MultipartSection section);
}