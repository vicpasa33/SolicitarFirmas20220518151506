namespace SolicitarFirmas.Models
{
    public class FormField
    {
            public string name { get; set; }
            public string label { get; set; }
            public string type { get; set; }
            public string identityId { get; set; }
            public string documentId { get; set; }
            public Position position { get; set; }
            public string value { get; set; }
            public string tooltip { get; set; }
            public bool required { get; set; }
            public int height { get; set; }
            public int width { get; set; }
            public string[] options { get; set; }
            public string radioButtonGroup { get; set; }
            public string validationPattern { get; set; }
            public string validationMessage { get; set; }
            public string maxLength { get; set; }
            public int occurrences { get; set; }
    }
}
