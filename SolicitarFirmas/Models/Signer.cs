namespace SolicitarFirmas.Models
{
    public class Signer
    {
        public string? clientReference { get; set; }
        public string? name { get; set; }
        public string? lastName { get; set; }
        public string? documentNumber { get; set; }
        public string? email { get; set; }
        public string? mobile { get; set; }
        public string? id { get; set; }
        public int? order { get; set; }
        public string? role { get; set; }
        public string? signMode { get; set; }
        public string? authenticationMethod { get; set; }
        public Tab? tabs { get; set; }
    }
}
