namespace FMData
{
    /// <summary>
    /// Create Response interface
    /// </summary>
    public interface IEditResponse : IResponse
    {
        /// <summary>
        /// The response object
        /// </summary>
        ActionResponse Response { get; set; }
    }
}
