using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using DocumentProcessor.Api.Configs;
using DocumentProcessor.Api.Functions;
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

        public async Task<string> GetBlobContentAsync(GetBlobContentRequest getBlobContentRequest)
        {
            try
            {
                var blobEndpoint = $"https://{_storageConfiguration.Account}.blob.core.windows.net";

                var blobServiceClient = new BlobServiceClient(new Uri(blobEndpoint), new DefaultAzureCredential());

                var blobClient = blobServiceClient.GetBlobContainerClient(_storageConfiguration.Container).GetBlobClient(getBlobContentRequest.BlobName);
                var downloadInformation = await blobClient.DownloadAsync();
                if (downloadInformation?.Value?.Content == null)
                {
                    return null;
                }

                string content;
                using (var reader = new StreamReader(downloadInformation.Value.Content, true))
                {
                    content = await reader.ReadToEndAsync();
                }

                return content;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error when getting content from the blob.");
            }

            return string.Empty;
        }
    }
}