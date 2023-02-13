namespace SolicitarFirmas.Models
{
    public class Signature
    {
        public string documentId { get; set; }
        public string identityId { get; set; }
        public Position[] positions { get; set; }
        public string type { get; set; }
        public Field[] fields { get; set; }
    }
}
