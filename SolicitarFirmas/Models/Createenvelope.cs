namespace SolicitarFirmas.Models
{
    public class Createenvelope
    {
        public string? clientReference { get; set; }
        public string? callBackUrl { get; set; }
        public bool? autoClose { get; set; }
        public Signer[]? signers { get; set; }
        public string? templateId { get; set; }
        public Document[]? documents { get; set; }
        public FormField[]? formFields { get; set; }
        public Customfield[]? customFields { get; set; }
        public Signature[]? signatures { get; set; }
        public Reminder? reminders { get; set; }
        public string? emailSubject { get; set; }
        public string? emailBlurb { get; set; }
        public string? brandId { get; set; }
    }
}
