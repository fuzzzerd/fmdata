namespace FMData
{
    public interface ICreateRequest<T> : IFileMakerRequest
    {
        T Data { get; set; }
    }
}