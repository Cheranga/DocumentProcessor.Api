namespace DocumentProcessor.Api.Functions
{
    public class ProcessDocumentRequest
    {
        public string Id { get; set; }
        public string Data { get; set; }
        public DocumentType DocumentType { get; set; }
        public string DataFormat { get; set; }
    }

    public enum DocumentType
    {
        Invoices,
        NewOrder
    }
}