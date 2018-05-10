namespace FMData
{
    public interface IEditRequest<T> : IFileMakerRequest
    {
        T Data { get; set; }
        string ModId { get; set; }
        string RecordId { get; set; }
    }
}