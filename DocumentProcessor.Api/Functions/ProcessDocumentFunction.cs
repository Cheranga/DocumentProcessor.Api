using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DocumentProcessor.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DocumentProcessor.Api.Functions
{
    public class ProcessDocumentFunction
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<ProcessDocumentFunction> _logger;
        private readonly long _maxRequestSize;

        public ProcessDocumentFunction(IDocumentService documentService, ILogger<ProcessDocumentFunction> logger)
        {
            _documentService = documentService;
            _logger = logger;

            if (!long.TryParse(Environment.GetEnvironmentVariable("MaxRequestSize"), out _maxRequestSize))
            {
                _maxRequestSize = 20000;
            }
        }

        [FunctionName(nameof(ProcessDocumentFunction))]
        public async Task<IActionResult> SaveDocumentAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents")]
            HttpRequest request,
            [ServiceBus("%ProcessDocumentTopic%")] IAsyncCollector<Message> messages)
        {
            try
            {
                var content = await new StreamReader(request.Body).ReadToEndAsync();
                var saveDocumentRequest = JsonConvert.DeserializeObject<ProcessDocumentRequest>(content);

                var isLarge = request.Body.Length >= _maxRequestSize;
                var status = await _documentService.ProcessDocumentAsync(saveDocumentRequest, isLarge, messages);
                
                if (status)
                {
                    return new OkResult();
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error when uploading the document.");
            }

            return new ObjectResult("Error when uploading the document.") {StatusCode = (int) HttpStatusCode.InternalServerError};
        }
    }
}