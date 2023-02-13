using Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace SolicitarFirmas.Models
{
    public class TrustCloudFileIds : TableEntity
    {
        public int? NoLineaCsv { get; set; }
        public string? Csv { get; set; }
        public string? TrustCloudFileId { get; set; }

        public TrustCloudFileIds(string PartitionKey, string RowKey) : base(PartitionKey, RowKey) { }
        public TrustCloudFileIds()
        {
            PartitionKey = TrustCloudFileId;
            RowKey = "Procesado";
        }
    }
}