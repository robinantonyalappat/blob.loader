
using blob.loader.Interfaces;
using blob.loader.Unused.Attributes;
using blob.loader.Unused.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace blob.loader.Controllers;

public class StreamFileUploadController : Controller
{
    readonly IStreamFileUploadService _streamFileUploadService;

    public StreamFileUploadController(IStreamFileUploadService streamFileUploadService)
    {
        _streamFileUploadService = streamFileUploadService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [ActionName("Index")]
    [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    //[DisableFormValueModelBinding]
    //[ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> SaveFileToPhysicalFolder()
    {
        if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
        {
            ModelState.AddModelError("File",
                $"The request couldn't be processed (Error 1).");
            // Log error

            return BadRequest(ModelState);
        }

        var boundary = HeaderUtilities.RemoveQuotes(
            MediaTypeHeaderValue.Parse(Request.ContentType).Boundary
        ).Value;

        var reader = new MultipartReader(boundary, Request.Body);

        try
        {
            var section = await reader.ReadNextSectionAsync();

            var response = string.Empty;

            if (await _streamFileUploadService.UploadFile(reader, section))
            {
                ViewBag.Message = "File Upload Successful";
            }
            else
            {
                ViewBag.Message = "File Upload Failed";
            }
        }
        catch (Exception ex)
        {
            //Log ex
            ViewBag.Message = "File Upload Failed";
        }
        return View();
    }
}