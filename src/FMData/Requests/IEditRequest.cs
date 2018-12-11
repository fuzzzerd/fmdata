namespace FMData
{
    /// <summary>
    /// Edit Request Interface
    /// </summary>
    /// <typeparam name="T">The type of object being edited.</typeparam>
    public interface IEditRequest<T> : IFileMakerRequest
    {
        /// <summary>
        /// The data representing this record.
        /// </summary>
        T Data { get; set; }
        /// <summary>
        /// FileMaker Modification Id to provide with the request.
        /// </summary>
        int ModId { get; set; }
        /// <summary>
        /// FileMaker record Id to be edited.
        /// </summary>
        int RecordId { get; set; }
    }
}