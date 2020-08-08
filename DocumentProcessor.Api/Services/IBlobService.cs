using System.Threading.Tasks;
using DocumentProcessor.Api.Functions;

namespace DocumentProcessor.Api.Services
{
    public interface IBlobService
    {
        Task<bool> UploadBlobAsync(string documentId, string data);
        Task<string> GetBlobContentAsync(GetBlobContentRequest getBlobContentRequest);
    }
}