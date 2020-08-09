using System.Threading.Tasks;

namespace DocumentProcessor.Api.Services
{
    public interface IBlobService
    {
        Task<bool> UploadBlobAsync(string documentId, string data);
    }
}