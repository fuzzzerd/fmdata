namespace FMData
{
    /// <summary>
    /// Methods and Properties all requests must implement.
    /// </summary>
    public interface IFileMakerRequest
    {
        string Layout { get; set; }
        string ResponseLayout { get; set; }

        string Script { get; set; }
        string ScriptParameter { get; set; }

        string PreRequestScript { get; set; }
        string PreRequestScriptParameter { get; set; }

        string PreSortScript { get; set; }
        string PreSortScriptParameter { get; set; }


        string SerializeRequest();
    }
}