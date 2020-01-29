Start of an in-memory fake implementation for testing data access. Scripts, Containers, Globals are ignored, though some of those could be implemented as well. The objective here is to provide a base that can be tested against without hitting FileMaker data stores.

```csharp
/// <summary>
/// Fakes an instance of the IFileMakerApiClient by using an in-memory "database" to handle requests.
/// </summary>
public class FMDataFake : FileMakerApiClientBase
{
    /// <summary>
    /// The In Memory "Database" to be used.
    /// </summary>
    private readonly Dictionary<string, List<object>> _dictionary;

    #region In-Memory Simulation Helpers
    /// <summary>
    /// Get the in-memory "database" list for the specified layout, or create a new one and return that.
    /// </summary>
    private List<Object> GetOrCreateForLayout(string layout)
    {
        if (!_dictionary.ContainsKey(layout))
        {
            _dictionary.Add(layout, new List<Object>());
        }

        var list = _dictionary[layout];
        return list;
    }

    /// <summary>
    /// Simulate a FileMaker Find against our in-memory "database."
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private IEnumerable<T> SimulateFileMakerFindOnInMemoryList<T>(IFindRequest<T> req) where T : class
    {
        var list = GetOrCreateForLayout(req.Layout);

        var properties = typeof(T).GetProperties();

        foreach (var item in list)
        {
            foreach (var query in req.Query)
            {
                var matches = GetMatchingProperties(item, query.QueryInstance);
                // only return the match if there is one and its not an Omit instance.
                if (matches.Count > 0 && query.Omit == false)
                {
                    yield return item as T;
                }
            }
        }

        yield break;
    }

    /// <summary>
    /// Compare two instances and return the list of matching properties, by Name, Type, and Value.
    /// </summary>
    private List<PropertyInfo> GetMatchingProperties(object source, object target)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        if (target == null)
            throw new ArgumentNullException("target");

        var sourceType = source.GetType();
        var sourceProperties = sourceType.GetProperties();
        var targetType = target.GetType();
        var targetProperties = targetType.GetProperties();

        var properties = (from s in sourceProperties
                          from t in targetProperties
                          where s.Name == t.Name &&
                                s.PropertyType == t.PropertyType &&
                                s.GetValue(source) == t.GetValue(target)
                          select s).ToList();
        return properties;
    }
    #endregion

    public FMDataFake(Dictionary<string, List<object>> dictionary = null) : base(new HttpClient(), new FMData.ConnectionInfo { Username = "u", Password = "p", FmsUri = "f", Database = "d" })
    {
        _dictionary = dictionary ?? new Dictionary<string, List<object>>();
    }
    public override void Dispose()
    {
        _dictionary.Clear();
    }

    public override Task<T> GetByFileMakerIdAsync<T>(string layout, int fileMakerId, Func<T, int, object> fmId = null, Func<T, int, object> fmMod = null)
    {
        var list = _dictionary[layout];
        return Task.FromResult(list[fileMakerId] as T);
    }

    public override Task<ICreateResponse> SendAsync<T>(ICreateRequest<T> req)
    {
        var list = GetOrCreateForLayout(req.Layout);
        list.Add(req.Data);

        var response = new CreateResponse()
        {
            Messages = new[] { new ResponseMessage { Code = "0", Message = "OK" } },
            Response = new ActionResponse { RecordId = list.IndexOf(req.Data) }
        } as ICreateResponse;

        return Task.FromResult(response);
    }

    public override Task<IResponse> SendAsync(IDeleteRequest req)
    {
        var list = GetOrCreateForLayout(req.Layout);
        list.RemoveAt(req.RecordId);

        var response = new BaseResponse
        {
            Messages = new[] { new ResponseMessage { Code = "0", Message = "OK" } },
        } as IResponse;

        return Task.FromResult(response);
    }

    public override Task<IEditResponse> SendAsync<T>(IEditRequest<T> req)
    {
        var list = GetOrCreateForLayout(req.Layout);

        list[req.RecordId] = req.Data;

        var response = new EditResponse
        {
            Messages = new[] { new ResponseMessage { Code = "0", Message = "OK" } },
            Response = new ActionResponse { RecordId = req.RecordId, ModId = 1 }
        } as IEditResponse;

        return Task.FromResult(response);
    }

    public override Task<IEnumerable<T>> SendAsync<T>(IFindRequest<T> req, Func<T, int, object> fmId, Func<T, int, object> modId)
    {
        return Task.FromResult(SimulateFileMakerFindOnInMemoryList(req));
    }

    #region TODO: Implement
    public override Task<IFindResponse<Dictionary<string, string>>> SendAsync(IFindRequest<Dictionary<string, string>> req)
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<T>> FindAsync<T>(string layout, Dictionary<string, string> req)
    {
        throw new NotImplementedException();
    }

    public override Task<IResponse> SetGlobalFieldAsync(string baseTable, string fieldName, string targetValue)
    {
        throw new NotImplementedException();
    }

    public override Task<IEditResponse> UpdateContainerAsync(string layout, int recordId, string fieldName, string fileName, int repetition, byte[] content)
    {
        throw new NotImplementedException();
    }

    protected override Task<byte[]> GetContainerOnClient(string containerEndPoint)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Factory Helpers
    protected override ICreateRequest<T> _createFactory<T>() => new CreateRequest<T>();
    protected override IDeleteRequest _deleteFactory() => new DeleteRequest();
    protected override IEditRequest<T> _editFactory<T>() => new EditRequest<T>();
    protected override IFindRequest<T> _findFactory<T>() => new FindRequest<T>();
    #endregion
}
```
