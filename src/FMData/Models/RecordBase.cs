namespace FMData
{
    /// <summary>
    /// Record Base
    /// </summary>
    /// <typeparam name="TypeFD">Type of Field Data</typeparam>
    /// <typeparam name="TypePD">Type of Portal Data</typeparam>
    public class RecordBase<TypeFD, TypePD>
    {
        /// <summary>
        /// Field Data
        /// </summary>
        public TypeFD FieldData { get; set; }
        /// <summary>
        /// Portal Data
        /// </summary>
        public TypePD PortalData { get; set; }
        /// <summary>
        /// FileMaker RecordId
        /// </summary>
        public int RecordId { get; set; }
        /// <summary>
        /// FileMaker Modification id
        /// </summary>
        public int ModId { get; set; }
    }
}