namespace FMData
{
    /// <summary>
    /// Delete Requeset Interface
    /// </summary>
    public interface IDeleteRequest : IFileMakerRequest
    {
        /// <summary>
        /// FileMaker record id to be deleted.
        /// </summary>
        int RecordId { get; set; }
    }
}