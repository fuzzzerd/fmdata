namespace FMData
{
    /// <summary>
    /// Delete Request Interface
    /// </summary>
    public interface IDeleteRequest : IFileMakerRequest
    {
        /// <summary>
        /// FileMaker record id to be deleted.
        /// </summary>
        int RecordId { get; set; }
    }
}
