namespace FMData
{
    public interface IDeleteRequest
    {
        string RecordId { get; set; }
        string Layout { get; set; }
    }
}