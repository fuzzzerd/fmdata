namespace FMData
{
    /// <summary>
    /// Methods and Properties all requests must implement.
    /// </summary>
    public interface IFileMakerRequest
    {
        string Layout { get; set; }
        string ResponseLayout { get; set; }

        string SerializeRequest();
    }
}