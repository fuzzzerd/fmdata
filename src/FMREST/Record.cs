namespace FMREST
{
    public class Record
    {
        public Dictionary<string, string> FieldData { get; set; }
        public Dictionary<string, string> PortalData { get; set; }
        public int RecordId { get; set; }
        public int ModId { get; set; }
    }
}