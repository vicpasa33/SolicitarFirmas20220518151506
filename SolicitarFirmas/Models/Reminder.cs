namespace SolicitarFirmas.Models
{
    public class Reminder
    {
        public int expireAfter { get; set; }
        public int expireWarn { get; set; }
        public int reminderDelay { get; set; }
        public int reminderFrequency { get; set; }
        public int numberOfRepitions { get; set; }
    }
}
