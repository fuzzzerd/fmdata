namespace FMData
{
    public interface ICreateRequest<T>
    {
        T Data { get; set; }
        string Layout { get; set; }
    }
}