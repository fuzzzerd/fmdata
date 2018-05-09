namespace FMData
{
    public class RecordBase<TypeFD, TypePD>
    {
        public TypeFD FieldData { get; set; }
        public TypePD PortalData { get; set; }
        public int RecordId { get; set; }
        public int ModId { get; set; }
    }
}