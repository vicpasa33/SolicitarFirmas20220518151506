namespace SolicitarFirmas.Models
{
    public class Field
    {
        public Position? position { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public bool required { get; set; }
        public string validationText { get; set; }
    }
}
