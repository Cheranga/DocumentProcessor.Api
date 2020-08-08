﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DocumentProcessor.Api.DTO;
using DocumentProcessor.Api.Functions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DocumentProcessor.Api.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IBlobService _blobService;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(IBlobService blobService, ILogger<DocumentService> logger)
        {
            _blobService = blobService;
            _logger = logger;
        }

        public async Task<bool> ProcessDocumentAsync(ProcessDocumentRequest request, bool isLargeFile, IAsyncCollector<Message> messages)
        {
            try
            {
                if (isLargeFile)
                {
                    var documentId = $"{request.Id}-{Guid.NewGuid():N}";
                    
                    await _blobService.UploadBlobAsync(documentId, JsonConvert.SerializeObject(request));

                    var processDocumentMessage = new ProcessDocumentMessage
                    {
                        RequestReferenceId = documentId
                    };

                    var message = new Message(Encoding.Default.GetBytes(JsonConvert.SerializeObject(processDocumentMessage)));
                    message.UserProperties.Add("DocumentType",request.DocumentType);
                    
                    await messages.AddAsync(message);
                }
                else
                {
                    var processDocumentMessage = new ProcessDocumentMessage
                    {
                        Request = request
                    };

                    var message = new Message(Encoding.Default.GetBytes(JsonConvert.SerializeObject(processDocumentMessage)));
                    message.UserProperties.Add("DocumentType", request.DocumentType);

                    await messages.AddAsync(message);
                }

                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error when processing the document.");
            }

            return false;
        }
    }
}
