namespace SolicitarFirmas.Models
{
    public class Document
    {
        public string Id { get; set; }
        public string Base64 { get; set; }
        public object Url { get; set; }
        public object DocumentARN { get; set; }
        public object DocumentReference { get; set; }
        public string FileName { get; set; }
        public object Tag { get; set; }
        public object FormFields { get; set; }
        public object Size { get; set; }
    }
}
