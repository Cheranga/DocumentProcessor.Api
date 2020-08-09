namespace DocumentProcessor.Api.DTO
{
    public class ProcessDocumentMessage
    {
        public string RequestReferenceId { get; set; }
        public ProcessDocumentRequest Request { get; set; }
    }
}