using System.Threading.Tasks;
using DocumentProcessor.Api.DTO;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;

namespace DocumentProcessor.Api.Services
{
    public interface IDocumentService
    {
        Task<bool> ProcessDocumentAsync(ProcessDocumentRequest request, bool isLargeFile, IAsyncCollector<Message> messages);
    }
}