namespace FMData
{
    /// <summary>
    /// Create Response interface
    /// </summary>
    public interface ICreateResponse : IResponse
    {
        /// <summary>
        /// The response object
        /// </summary>
        CreateResponseType Response { get; set; }
    }

    /// <summary>
    /// Create Response Type holder for the nested 'Response' Type
    /// </summary>
    public class CreateResponseType
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