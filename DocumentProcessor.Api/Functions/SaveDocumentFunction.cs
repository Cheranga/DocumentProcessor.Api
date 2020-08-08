using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Azure.Identity;
using Azure.Storage.Blobs;
using DocumentProcessor.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DocumentProcessor.Api.Functions
{
    public class SaveDocumentFunction
    {
        private readonly IBlobService _blobService;
        private readonly ILogger<SaveDocumentFunction> _logger;

        public SaveDocumentFunction(IBlobService blobService, ILogger<SaveDocumentFunction> logger)
        {
            _blobService = blobService;
            _logger = logger;
        }

        [FunctionName(nameof(SaveDocumentFunction))]
        public async Task<IActionResult> SaveDocumentAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents")]
            HttpRequest request)
        {
            try
            {
                var content = await new StreamReader(request.Body).ReadToEndAsync();
                var saveDocumentRequest = JsonConvert.DeserializeObject<SaveBlobRequest>(content);

                var status = await _blobService.UploadBlobAsync(saveDocumentRequest);
                if (status)
                {
                    return new OkResult();
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error when uploading the document.");
            }

            return new ObjectResult("Error when uploading the document.") { StatusCode = (int)HttpStatusCode.InternalServerError };
        }
    }
}