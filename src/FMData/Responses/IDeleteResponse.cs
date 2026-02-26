namespace FMData
{
    /// <summary>
    /// Delete Response interface
    /// </summary>
    public interface IDeleteResponse : IResponse
    {
        /// <summary>
        /// The response object
        /// </summary>
        ActionResponse Response { get; set; }
    }
}
