namespace FMData.Responses
{
    public class RecordBase<FD, PD>
    {
        public FD FieldData { get; set; }
        public PD PortalData { get; set; }
        public int RecordId { get; set; }
        public int ModId { get; set; }
    }
}