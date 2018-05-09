namespace FMData.Rest.Requests
{
    public interface ICreateRequest<T>
    {
        T Data { get; set; }
    }
}