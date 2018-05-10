namespace FMData
{
    public interface IDeleteRequest : IFileMakerRequest
    {
        string RecordId { get; set; }
    }
}