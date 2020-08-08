using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DocumentProcessor.Api.Functions
{
    public class SaveDocumentRequest
    {
        public string Id { get; set; }
        public string Data { get; set; }
    }

    public class SaveDocumentFunction
    {
        private readonly ILogger<SaveDocumentFunction> _logger;

        public SaveDocumentFunction(ILogger<SaveDocumentFunction> logger)
        {
            _logger = logger;
        }

        [FunctionName("GetSasFunction")]
        public async Task<IActionResult> GetSasAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents/token")]HttpRequest request)
        {
            try
            {
                var sasUri = await GetUserDelegationSasBlob("ccclaimchecksg", "largefiles", "Customers_10.csv");

                return new OkObjectResult(sasUri.ToString());
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error occured.");
            }

            return new InternalServerErrorResult();
        }

        [FunctionName("SaveDocumentFunction")]
        public async Task<IActionResult> SaveDocumentAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents")]HttpRequest request)
        {
            try
            {
                var content = await new StreamReader(request.Body).ReadToEndAsync();
                var saveDocumentRequest = JsonConvert.DeserializeObject<SaveDocumentRequest>(content);
                var documentBytes = Encoding.Default.GetBytes(saveDocumentRequest.Data);

                var blobEndpoint = string.Format("https://ccclaimchecksg.blob.core.windows.net");

                // Create a new Blob service client with Azure AD credentials.  
                var blobClient = new BlobServiceClient(new Uri(blobEndpoint), new DefaultAzureCredential());

                var blobContainerClient = blobClient.GetBlobContainerClient("largefiles");

                using (var memoryStream = new MemoryStream(documentBytes))
                {
                    var operation = await blobContainerClient.UploadBlobAsync(saveDocumentRequest.Id, memoryStream);

                    return new OkResult();
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error when uploading the document.");

                return new ObjectResult(exception){StatusCode = (int)(HttpStatusCode.InternalServerError)};
            }
        }

        [FunctionName("GetDocumentDataFunction")]
        public async Task<IActionResult> GetDocumentDataAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")]HttpRequest request)
        {
            try
            {
                _logger.LogInformation("Getting document data.");
                string documentName = request.Query["document"];

                var blobEndpoint = "https://ccclaimchecksg.blob.core.windows.net";

                // Create a new Blob service client with Azure AD credentials.  
                var blobServiceClient = new BlobServiceClient(new Uri(blobEndpoint), new DefaultAzureCredential());
                

                var blobClient = blobServiceClient.GetBlobContainerClient("largefiles").GetBlobClient(documentName);
                var downloadInformation = await blobClient.DownloadAsync();
                if (downloadInformation?.Value?.Content == null)
                {
                    return new NotFoundObjectResult("File does not exist");
                }

                string content;
                using (var reader = new StreamReader(downloadInformation.Value.Content, true))
                {
                    content = await reader.ReadToEndAsync();
                }

                return new OkObjectResult(new {Content = content});

            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error when uploading the document.");

                return new ObjectResult(exception) { StatusCode = (int)(HttpStatusCode.InternalServerError) };
            }
        }


        private async Task<Uri> GetUserDelegationSasBlob(string accountName, string containerName, string blobName)
        {
            // Construct the blob endpoint from the account name.
            var blobEndpoint = string.Format("https://{0}.blob.core.windows.net", accountName);

            // Create a new Blob service client with Azure AD credentials.  
            var blobClient = new BlobServiceClient(new Uri(blobEndpoint),
                                                                    new DefaultAzureCredential());

            // Get a user delegation key for the Blob service that's valid for seven days.
            // You can use the key to generate any number of shared access signatures over the lifetime of the key.
            UserDelegationKey key = await blobClient.GetUserDelegationKeyAsync(DateTimeOffset.UtcNow,
                                                                                DateTimeOffset.UtcNow.AddDays(7));


            // Create a SAS token that's valid for one hour.
            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };

            // Specify read permissions for the SAS.
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Use the key to get the SAS token.
            var sasToken = sasBuilder.ToSasQueryParameters(key, accountName).ToString();

            // Construct the full URI, including the SAS token.
            var fullUri = new UriBuilder()
            {
                Scheme = "https",
                Host = string.Format("{0}.blob.core.windows.net", accountName),
                Path = string.Format("{0}/{1}", containerName, blobName),
                Query = sasToken
            };

            
            return fullUri.Uri;
        }

    }
}