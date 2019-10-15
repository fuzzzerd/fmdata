namespace FMData
{
    /// <summary>
    /// Type holder for the nested 'Response' Type
    /// </summary>
    public class ActionResponse
    {
        /// <summary>
        /// Record Id that was created.
        /// </summary>
        public int RecordId { get; set; }

        /// <summary>
        /// Modification Id of the record.
        /// </summary>
        public int ModId { get; set; }

        /// <summary>
        /// Script Error (if any)
        /// </summary>
        public int ScriptError { get; set; }
    }
}