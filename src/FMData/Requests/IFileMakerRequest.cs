namespace FMData
{
    /// <summary>
    /// Methds all requests must implement.
    /// </summary>
    public interface IFileMakerRequest
    {
        string Layout { get; set; }


        string SerializeRequest();
    }
}