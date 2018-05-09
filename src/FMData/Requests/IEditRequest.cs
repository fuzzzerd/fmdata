namespace FMData
{
    public interface IEditRequest<T>
    {
        T Data { get; set; }
        string ModId { get; set; }
        string RecordId { get; set; }
        string Layout { get; set; }
    }
}