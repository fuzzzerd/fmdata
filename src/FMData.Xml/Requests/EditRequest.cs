namespace FMData.Xml.Requests
{
    /// <summary>
    /// Create Request Wrapper
    /// </summary>
    /// <typeparam name="T">The type to create.</typeparam>
    public class EditRequest<T> : RequestBase, IEditRequest<T>
    {
        /// <summary>
        /// The field data for the create request.
        /// </summary>
        public T Data { get; set; }
        /// <summary>
        /// Modification Id
        /// </summary>
        public string ModId { get; set; }
        /// <summary>
        /// RecordId
        /// </summary>
        public string RecordId { get; set; }
    }
}