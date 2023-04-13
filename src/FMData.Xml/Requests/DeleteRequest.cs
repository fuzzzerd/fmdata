namespace FMData.Xml.Requests
{
    /// <summary>
    /// Create Request Wrapper
    /// </summary>
    public class DeleteRequest : RequestBase, IDeleteRequest
    {
        /// <summary>
        /// RecordId
        /// </summary>
        public int RecordId { get; set; }

        /// <summary>
        /// Serialize the request to the format needed for filemaker to accept it.
        /// </summary>
        /// <returns>String representation of the request.</returns>
        public override string SerializeRequest()
        {
            var requestContent = $"-delete&-lay={Layout}&-recid={RecordId}";
            return requestContent;
        }
    }
}
