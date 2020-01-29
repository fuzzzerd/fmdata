Start of an in-memory fake implementation for testing data access. Scripts, Containers, Globals are ignored, though some of those could be implemented as well. The objective here is to provide a base that can be tested against without hitting FileMaker data stores.

```csharp
/// <summary>
/// Fakes an instance of the IFileMakerApiClient by using an in-memory "database" to handle requests.
/// </summary>
public class FMDataFake : FileMakerApiClientBase
{
    /// <summary>
    /// In Memory "Database"
    /// </summary>
    private readonly Dictionary<string, List<object>> _dictionary;

    private List<Object> GetOrCreateForLayout(string layout)
    {
        if (!_dictionary.ContainsKey(layout))
        {
            _dictionary.Add(layout, new List<Object>());
        }

        var list = _dictionary[layout];
        return list;
    }
    public FMDataFake(Dictionary<string, List<object>> dictionary = null) : base(new HttpClient(), new FMData.ConnectionInfo { Username = "u", Password = "p", FmsUri = "f", Database = "d" })
    {
        _dictionary = dictionary ?? new Dictionary<string, List<object>>();
    }

    public override void Dispose()
    {
        _dictionary.Clear();
    }

    public override Task<IEnumerable<T>> FindAsync<T>(string layout, Dictionary<string, string> req)
    {
        throw new NotImplementedException();
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

  public override Task<IFindResponse<Dictionary<string, string>>> SendAsync(IFindRequest<Dictionary<string, string>> req)
  {
      throw new NotImplementedException();
  }

  public override Task<IEnumerable<T>> SendAsync<T>(IFindRequest<T> req, Func<T, int, object> fmId, Func<T, int, object> modId)
  {
      var list = GetOrCreateForLayout(req.Layout);

      foreach (var item in list)
      {

      }
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

  protected override ICreateRequest<T> _createFactory<T>() => new CreateRequest<T>();
  protected override IDeleteRequest _deleteFactory() => new DeleteRequest();
  protected override IEditRequest<T> _editFactory<T>() => new EditRequest<T>();
  protected override IFindRequest<T> _findFactory<T>() => new FindRequest<T>();
}
```
