using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DocumentProcessor.Api.Functions
{
    public class ProcessDocumentRequest
    {
        public string Id { get; set; }
        public string Data { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DocumentType DocumentType { get; set; }
        public string DataFormat { get; set; }
    }

    public enum DocumentType
    {
        Invoices,
        NewOrder
    }
}