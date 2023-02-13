namespace SolicitarFirmas.Models
{
    public class Search
    {
        public string trustCloudFileId { get; set; }
        public string providerId { get; set; }
        public string status { get; set; }
        public object statusMessage { get; set; }
        public Customfield[]? customFields { get; set; }
    }
}