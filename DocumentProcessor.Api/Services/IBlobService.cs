using System.Threading.Tasks;
using DocumentProcessor.Api.Functions;

namespace DocumentProcessor.Api.Services
{
    public interface IBlobService
    {
        Task<bool> UploadBlobAsync(SaveBlobRequest saveBlobRequest);
        Task<string> GetBlobContentAsync(GetBlobContentRequest getBlobContentRequest);
    }
}