using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using DocumentProcessor.Api.Configs;
using Microsoft.Extensions.Logging;

namespace DocumentProcessor.Api.Services
{
    public class BlobService : IBlobService
    {
        private readonly ILogger<BlobService> _logger;
        private readonly SecureStorageConfiguration _storageConfiguration;

        public BlobService(SecureStorageConfiguration storageConfiguration, ILogger<BlobService> logger)
        {
            _storageConfiguration = storageConfiguration;
            _logger = logger;
        }

        public async Task<bool> UploadBlobAsync(string documentId, string data)
        {
            try
            {
                var documentBytes = Encoding.Default.GetBytes(data);

                var blobEndpoint = $"https://{_storageConfiguration.Account}.blob.core.windows.net";

                var blobClient = new BlobServiceClient(new Uri(blobEndpoint), new DefaultAzureCredential());

                var blobContainerClient = blobClient.GetBlobContainerClient(_storageConfiguration.Container);

                using (var memoryStream = new MemoryStream(documentBytes))
                {
                    await blobContainerClient.UploadBlobAsync(documentId, memoryStream);
                    return true;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error when uploading document.");
            }

            return false;
        }
    }
}