using System;
using System.Net;
using System.Threading.Tasks;
using DocumentProcessor.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DocumentProcessor.Api.Functions
{
    public class GetBlobContentFunction
    {
        private readonly IBlobService _blobService;
        private readonly ILogger<GetBlobContentFunction> _logger;

        public GetBlobContentFunction(IBlobService blobService, ILogger<GetBlobContentFunction> logger)
        {
            _blobService = blobService;
            _logger = logger;
        }

        [FunctionName(nameof(GetBlobContentFunction))]
        public async Task<IActionResult> GetDocumentDataAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")]
            HttpRequest request)
        {
            try
            {
                _logger.LogInformation("Getting document data.");
                string documentName = request.Query["document"];

                var getBlobContentRequest = new GetBlobContentRequest
                {
                    BlobName = documentName
                };

                var content = await _blobService.GetBlobContentAsync(getBlobContentRequest);
                if (string.IsNullOrEmpty(content))
                {
                    return new NotFoundObjectResult("The file could not be found or the file content is empty.");
                }

                return new OkObjectResult(new
                {
                    Content = content
                });
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error when uploading the document.");

                return new ObjectResult(exception) {StatusCode = (int) HttpStatusCode.InternalServerError};
            }
        }
    }
}