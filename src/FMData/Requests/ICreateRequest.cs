namespace FMData
{
    /// <summary>
    /// Create Record Request
    /// </summary>
    /// <typeparam name="T">The type of record to be created.</typeparam>
    public interface ICreateRequest<T> : IFileMakerRequest
    {
        /// <summary>
        /// The data to put in the new record.
        /// </summary>
        T Data { get; set; }
    }
}