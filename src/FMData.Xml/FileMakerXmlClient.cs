using FMData.Xml.Requests;
using FMData.Xml.Responses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FMData.Xml
{
    /// <summary>
    /// FileMaker Xml API Client Implementation.
    /// </summary>
    public class FileMakerXmlClient : FileMakerApiClientBase, IFileMakerApiClient
    {
        #region Request Factories
        /// <summary>
        /// Factory to get a new Create Request of the correct type.
        /// </summary>
        protected override ICreateRequest<T> _createFactory<T>() => new CreateRequest<T>();
        /// <summary>
        /// Factory to get a new Edit Request of the correct type.
        /// </summary>
        protected override IEditRequest<T> _editFactory<T>() => new EditRequest<T>();
        /// <summary>
        /// Factory to get a new Find Request of the correct type.
        /// </summary>
        protected override IFindRequest<T> _findFactory<T>() => new FindRequest<T>();
        /// <summary>
        /// Factory to get a new Delete Request of the correct type.
        /// </summary>
        protected override IDeleteRequest _deleteFactory() => new DeleteRequest();
        #endregion

        private readonly XNamespace _ns = "http://www.filemaker.com/xml/fmresultset";

        private List<string> _globalsToAdd = new List<string>();

        #region Constructors
        /// <summary>
        /// FM Data Constructor. Injects a new plain old <see ref="HttpClient"/> instance to the class.
        /// </summary>
        /// <param name="fmsUri">FileMaker Server HTTP Uri Endpoint.</param>
        /// <param name="file">Name of the FileMaker Database to connect to.</param>
        /// <param name="user">Account to connect with.</param>
        /// <param name="pass">Account to connect with.</param>
        /// <remarks>Pass through constructor with no real body used for injection.</remarks>
        public FileMakerXmlClient(string fmsUri, string file, string user, string pass)
            : this(new HttpClient(new HttpClientHandler
            {
                Credentials = new NetworkCredential(user, pass)
            }),
                    new ConnectionInfo { FmsUri = fmsUri, Database = file, Username = user, Password = pass }) { }

        /// <summary>
        /// FM Data Constructor. Injects a new plain old <see ref="HttpClient"/> instance to the class.
        /// </summary>
        /// <param name="client">An <see ref="HttpClient"/> instance to utilize for the liftime of this Data Client.</param>
        /// <param name="fmsUri">FileMaker Server HTTP Uri Endpoint.</param>
        /// <param name="file">Name of the FileMaker Database to connect to.</param>
        /// <param name="user">Account to connect with.</param>
        /// <param name="pass">Account to connect with.</param>
        public FileMakerXmlClient(HttpClient client, string fmsUri, string file, string user, string pass)
            : this(client, new ConnectionInfo { FmsUri = fmsUri, Database = file, Username = user, Password = pass }) { }

        /// <summary>
        /// FM Data Constructor with HttpClient and ConnectionInfo. Useful for Dependency Injection situations
        /// </summary>
        /// <param name="client">The HttpClient instance to use.</param>
        /// <param name="conn">The connection information for FMS.</param>
        public FileMakerXmlClient(HttpClient client, ConnectionInfo conn) : base(client, conn) { }
        #endregion

        #region Special Implementations
        /// <summary>
        /// Gets a record by its FileMaker Id and Layout.
        /// </summary>
        public override Task<T> GetByFileMakerIdAsync<T>(string layout, int fileMakerId, Func<T, int, object> fmId = null, Func<T, int, object> modId = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Find a record using a dictionary of input parameters.
        /// </summary>
        public override Task<IFindResponse<Dictionary<string, string>>> SendAsync(IFindRequest<Dictionary<string, string>> req)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Finds records using a layout and a dictionary of strings as criteria.
        /// </summary>
        public override Task<IEnumerable<T>> FindAsync<T>(string layout, Dictionary<string, string> req)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region SendAsync Implementations
        /// <summary>
        /// Executes a Create Record Request
        /// </summary>
        /// <typeparam name="T">The projected type to be created.</typeparam>
        /// <param name="req">The request record command.</param>
        /// <returns>A response containing the results of the operation.</returns>
        public async override Task<ICreateResponse> SendAsync<T>(ICreateRequest<T> req)
        {
            HttpResponseMessage response = await ExecuteRequestAsync(req);

            if (response.IsSuccessStatusCode)
            {
                // process response data return OK
                var resp = new CreateResponse
                {
                    Messages = new List<ResponseMessage> { new ResponseMessage { Code = "", Message = "OK" } }
                };
                return resp;
            }

            throw new Exception("Unable to complete request");
        }

        /// <summary>
        /// Executes a delete request.
        /// </summary>
        public override async Task<IResponse> SendAsync(IDeleteRequest req)
        {
            HttpResponseMessage response = await ExecuteRequestAsync(req);

            if (response.IsSuccessStatusCode)
            {
                // process response data return OK
                var resp = new CreateResponse
                {
                    Messages = new List<ResponseMessage> { new ResponseMessage { Code = "", Message = "OK" } }
                };
                return resp;
            }

            throw new Exception("Unable to complete request");
        }

        /// <summary>
        /// Executes an edit request.
        /// </summary>
        public override async Task<IEditResponse> SendAsync<T>(IEditRequest<T> req)
        {
            HttpResponseMessage response = await ExecuteRequestAsync(req);

            if (response.IsSuccessStatusCode)
            {
                // process response data return OK
                var resp = new EditResponse
                {
                    Messages = new List<ResponseMessage> { new ResponseMessage { Code = "", Message = "OK" } }
                };
                return resp;
            }

            throw new Exception("Unable to complete request");
        }

        /// <summary>
        /// Executes a Find Request and returns the matching objects projected by the type parameter.
        /// </summary>
        /// <typeparam name="T">The type to project the results against.</typeparam>
        /// <param name="req">The Find Request Command.</param>
        /// <param name="fmId">The function to map FileMaker Record Ids to an instance of T.</param>
        /// <param name="modId">The function to map FileMaker Modid to an instance of T</param>
        /// <returns>The projected results matching the find request.</returns>
        public override async Task<IEnumerable<T>> SendAsync<T>(
            IFindRequest<T> req,
            Func<T, int, object> fmId,
            Func<T, int, object> modId)
        {
            HttpResponseMessage response = await ExecuteRequestAsync(req);

            if (response.IsSuccessStatusCode)
            {
                // process response data return OK
                var xdoc = XDocument.Load(await response.Content.ReadAsStreamAsync());

                // act
                var metadata = xdoc
                    .Descendants(_ns + "metadata")
                    .Elements(_ns + "field-definition")
                    .ToDictionary(
                        k => k.Attribute("name").Value,
                        v => v.Attribute("result").Value
                    );

                // load in relatedSet metadatas
                var relatedMeta = xdoc
                    .Descendants(_ns + "metadata")
                    .Elements(_ns + "relatedset-definition")
                    .ToDictionary(
                        k => k.Attribute("table").Value,
                        val => val.Elements(_ns + "field-definition")
                            .ToDictionary(
                                k => k.Attribute("name").Value,
                                v => v.Attribute("result").Value
                            )
                    );

                var dict = new Dictionary<string, string>();
                var records = xdoc
                    .Descendants(_ns + "resultset")
                    .Elements(_ns + "record")
                    .Select(r => new RecordBase<T, Dictionary<string, IEnumerable<Dictionary<string, object>>>>
                    {
                        RecordId = Convert.ToInt32(r.Attribute("record-id").Value),
                        ModId = Convert.ToInt32(r.Attribute("mod-id").Value),
                        FieldData = FieldDataToDictionary(metadata, r.Elements(_ns + "field")).ToObject<T>(),
                        PortalData = r.Elements(_ns + "relatedset")
                            .ToDictionary(
                                k => k.Attribute("table").Value,
                                v => v.Elements(_ns + "record")
                                    .Select(rc =>
                                        FieldDataToDictionary(
                                            relatedMeta[v.Attribute("table").Value],
                                            rc.Elements(_ns + "field")
                                        )
                                    )
                            )
                    })
                    .ToList(); // make sure to ToList here since if we don't subsequent setting of child fields/properties are lost for every time its enumerated again

                // handle record and modid
                foreach (var record in records)
                {
                    fmId?.Invoke(record.FieldData, record.RecordId);
                    modId?.Invoke(record.FieldData, record.ModId);

                    // TODO: update each record's FieldData instance with the contents of its PortalData
                    var portals = typeof(T).GetTypeInfo().DeclaredProperties.Where(p => p.GetCustomAttribute<PortalDataAttribute>() != null);
                    foreach (var portal in portals)
                    {
                        var portalDataAttr = portal.GetCustomAttribute<PortalDataAttribute>();
                        var namedPortal = portalDataAttr.NamedPortalInstance;
                        var portalInstanceType = portal.PropertyType.GetTypeInfo().GenericTypeArguments[0];
                        var pt = portal.PropertyType;

                        var dataPortal = record.PortalData[namedPortal].ToList();

                        // .ToList() here so we iterate on a different copy of the collection
                        // which allows for calling add/remove on the list ;) clever
                        // https://stackoverflow.com/a/26864676/86860 - explination 
                        // https://stackoverflow.com/a/604843/86860 - solution
                        foreach (var row in dataPortal.ToList())
                        {
                            foreach (var kvp in row.ToList())
                            {
                                if (kvp.Key.Contains(portalDataAttr.TablePrefixFieldNames + "::"))
                                {
                                    row.Add(kvp.Key.Replace(portalDataAttr.TablePrefixFieldNames + "::", ""), kvp.Value);
                                    row.Remove(kvp.Key);
                                }
                            }
                        }

                        var x = dataPortal.Select(portalRow => portalRow.ToObject(portalInstanceType));
                        var y = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(portalInstanceType));
                        foreach (var z in x) y.Add(z);
                        portal.SetValue(record.FieldData, y);
                    }
                }

                var results = records.Select(r => r.FieldData);

                // make container processing part of the request, IF specified in the original request.
                if (req.LoadContainerData) 
                {
                    await ProcessContainers(results);
                }

                return results;
            }

            return null;
        }
        #endregion

        /// <summary>
        /// Adds a 'set global field' to a list that will be depleted on the next actual api call (find, edit, create, delete).
        /// </summary>
        /// <param name="baseTable"></param>
        /// <param name="fieldName"></param>
        /// <param name="targetValue"></param>
        /// <returns>Always returns null, since we don't actually get a response at this point.</returns>
        public override Task<IResponse> SetGlobalFieldAsync(string baseTable, string fieldName, string targetValue)
        {
            _globalsToAdd.Add(Uri.EscapeDataString($"&{baseTable}::{fieldName}.global={targetValue}"));
            return null;
        }

        /// <summary>
        /// Upload data to a container field.
        /// TODO: Workaround with B64 encoding and container auto-enter?
        /// </summary>
        public override Task<IEditResponse> UpdateContainerAsync(string layout, int recordId, string fieldName, string fileName, int repetition, byte[] content)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Utility method that must be overridden in implemenations. Takes a containerfield url and populpates a byte array utilizing the instance's http client.
        /// </summary>
        /// <param name="containerEndPoint">The container field to load.</param>
        /// <returns>An array of bytes with the data from the container field.</returns>
        protected override async Task<byte[]> GetContainerOnClient(string containerEndPoint)
        {
            var data = await _client.GetAsync(containerEndPoint);
            var dataBytes = await data.Content.ReadAsByteArrayAsync();
            return dataBytes;
        }

        #region Private Helpers and utility methods
        /// <summary>
        /// Execute the POST request to FMS XML
        /// </summary>
        /// <param name="req">The Request To Execute.</param>
        /// <returns>The HttpResponseMessage From The Request.</returns>
        private async Task<HttpResponseMessage> ExecuteRequestAsync(IFileMakerRequest req)
        {
            var url = _fmsUri + "/fmi/xml/fmresultset.xml";

            var requestContent = req.SerializeRequest();

            var globals = string.Join("", _globalsToAdd);
            _globalsToAdd.Clear();

            // append fileName to request since thats not represented in the request itself
            // append globals
            var sContent = requestContent + globals + $"&-db={_fileName}";

            var httpRequestContent = new StringContent(sContent);

            var response = await _client.PostAsync(url, httpRequestContent);
            return response;
        }

        /// <summary>
        /// Convert a FMS XML Field Data into a <see cref="Dictionary{String, Object}"/>
        /// </summary>
        /// <param name="metadata">The Metadata for this FieldSet</param>
        /// <param name="enumerable">The collection of XElements containing the FieldData</param>
        private Dictionary<string, object> FieldDataToDictionary(
            Dictionary<string, string> metadata,
            IEnumerable<XElement> enumerable)
        {
            return enumerable.ToDictionary(
                k => k.Attribute("name").Value,
                v =>
                {
                    switch (metadata[v.Attribute("name").Value])
                    {
                        case "number":
                            return Convert.ChangeType(v.Value, typeof(int));
                        case "date":
                        case "timestamp":
                            return Convert.ChangeType(v.Value, typeof(DateTime));
                        case "time":
                            return Convert.ChangeType(v.Value, typeof(TimeSpan));
                        default:
                            return v.Value;
                    }
                });
        }
        #endregion

        #region IDisposable Implementation
        /// <summary>
        /// Dispose resources opened for this instance of the data client.
        /// </summary>
        public override void Dispose()
        {
            if (_client != null)
            {
                // dispose our injected http client
                _client.Dispose();
            }
        }
        #endregion
    }
}