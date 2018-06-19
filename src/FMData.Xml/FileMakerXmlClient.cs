using FMData.Xml.Requests;
using FMData.Xml.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FMData.Xml
{
    public class FileMakerXmlClient : FileMakerApiClientBase, IFileMakerApiClient
    {
        private readonly XNamespace _ns = "http://www.filemaker.com/xml/fmresultset";
        private readonly HttpClient _client;
        private readonly string _fmsUri;
        private readonly string _fileName;
        private readonly string _userName;
        private readonly string _password;

        #region Constructors
        /// <summary>
        /// FM Data Constructor. Injects a new plain old <see ref="HttpClient"/> instance to the class.
        /// </summary>
        /// <param name="fmsUri">FileMaker Server HTTP Uri Endpoint.</param>
        /// <param name="file">Name of the FileMaker Database to connect to.</param>
        /// <param name="user">Account to connect with.</param>
        /// <param name="pass">Account to connect with.</param>
        /// <param name="initialLayout">Layout to use for the initial authentication request.</param>
        /// <remarks>Pass through constructor with no real body used for injection.</remarks>
        public FileMakerXmlClient(string fmsUri, string file, string user, string pass, string initialLayout)
            : this(new HttpClient(), fmsUri, file, user, pass, initialLayout) { }

        /// <summary>
        /// FM Data Constructor. Injects a new plain old <see ref="HttpClient"/> instance to the class.
        /// </summary>
        /// <param name="client">An <see ref="HttpClient"/> instance to utilize for the liftime of this Data Client.</param>
        /// <param name="fmsUri">FileMaker Server HTTP Uri Endpoint.</param>
        /// <param name="file">Name of the FileMaker Database to connect to.</param>
        /// <param name="user">Account to connect with.</param>
        /// <param name="pass">Account to connect with.</param>
        /// <param name="initialLayout">Layout to use for the initial authentication request.</param>
        public FileMakerXmlClient(HttpClient client, string fmsUri, string file, string user, string pass, string initialLayout)
        {
            _client = client;

            _fmsUri = fmsUri;
            // trim out the trailing slash if they included it
            if (_fmsUri.EndsWith("/", StringComparison.CurrentCultureIgnoreCase))
            {
                _fmsUri = fmsUri.Substring(0, fmsUri.Length - 1);
            }
            _fileName = file;
            _userName = user;
            _password = pass;
        }
        #endregion

        public override async Task<IResponse> CreateAsync<T>(T input)
        {
            // setup 
            var layout = "layout";

            var dictionary = input.GetType().GetTypeInfo().DeclaredProperties
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(input, null));

            var url = _fmsUri + "/fmi/xml/fmresultset.xml";

            var stringContent = string.Join("", dictionary.Select(i => $"&{Uri.EscapeUriString(i.Key)}={Uri.EscapeUriString(i.Value.ToString())}"));
            var httpRequestContent = new StringContent($"-new&-db={_fileName}&-lay={layout}{stringContent}");

            var response = await _client.PostAsync(url, httpRequestContent);


            if (response.IsSuccessStatusCode)
            {
                // process response data return OK
                return new BaseResponse("", "OK");
            }

            throw new Exception("Unable to complete request");
            
        }

        public override Task<IResponse> CreateAsync<T>(string layout, T input)
        {
            throw new NotImplementedException();
        }

        public override Task<IResponse> SendAsync<T>(ICreateRequest<T> req)
        {
            throw new NotImplementedException();
        }

        public override Task<IResponse> SendAsync(IDeleteRequest req)
        {
            throw new NotImplementedException();
        }

        public override Task<IResponse> DeleteAsync(int recId, string layout)
        {
            throw new NotImplementedException();
        }

        public override Task<IResponse> SendAsync<T>(IEditRequest<T> req)
        {
            throw new NotImplementedException();
        }

        public override Task<IResponse> EditAsync<T>(int recordId, T input)
        {
            throw new NotImplementedException();
        }

        public override Task<IResponse> EditAsync<T>(string layout, int recordId, T input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Strongly typed find request.
        /// </summary>
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="layout">The name of the layout to run this request on.</param>
        /// <param name="input">The object with properties to map to the find request.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public override Task<IEnumerable<T>> FindAsync<T>(string layout, T input) => SendAsync((IFindRequest<T>)new FindRequest<T>() { Layout = layout, Query = new List<T>() { input } });


        public override async Task<IEnumerable<T>> SendAsync<T>(IFindRequest<T> req, Func<T, int, object> fmId = null)
        {
            var url = _fmsUri + "/fmi/xml/fmresultset.xml";

            var dictionary = req.Query.First().AsDictionary(false);

            var stringContent = string.Join("", dictionary.Select(i => $"&{Uri.EscapeUriString(i.Key)}={Uri.EscapeUriString(i.Value.ToString())}"));
            var httpRequestContent = new StringContent($"–find&-db={_fileName}&-lay={req.Layout}{stringContent}");

            var response = await _client.PostAsync(url, httpRequestContent);

            if (response.IsSuccessStatusCode)
            {
                // process response data return OK
                var xdoc = XDocument.Load(await response.Content.ReadAsStreamAsync());

                // act
                var dict = new Dictionary<string, string>();
                var records = xdoc
                    .Descendants(_ns + "resultset")
                    .Elements(_ns + "record")
                    .Select(r => new RecordBase<T,T>
                    {
                        RecordId = Convert.ToInt32(r.Attribute("record-id").Value),
                        ModId = Convert.ToInt32(r.Attribute("mod-id").Value),
                        FieldData = r.Elements(_ns + "field")
                            .ToDictionary(
                                k => k.Attribute("name").Value,
                                v => v.Attribute("name").Value == "length" ? Convert.ChangeType(v.Value,typeof(int)) : v.Value
                            ).ToObject<T>()
                    });

                return records.Select(r => r.FieldData);
            }

            return null;
        }

        public override Task<IFindResponse<Dictionary<string, string>>> SendAsync(IFindRequest<Dictionary<string, string>> req)
        {
            throw new NotImplementedException();
        }

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

        public override Task<IEnumerable<T>> FindAsync<T>(string layout, Dictionary<string, string> req)
        {
            throw new NotImplementedException();
        }

        public override Task<IResponse> EditAsync(int recordId, string layout, Dictionary<string, string> editValues)
        {
            throw new NotImplementedException();
        }

        protected override ICreateRequest<T> _createFactory<T>()
        {
            throw new NotImplementedException();
        }

        protected override IEditRequest<T> _editFactory<T>()
        {
            throw new NotImplementedException();
        }

        protected override IFindRequest<T> _findFactory<T>()
        {
            throw new NotImplementedException();
        }

        protected override IDeleteRequest _deleteFactory()
        {
            throw new NotImplementedException();
        }

        public override Task<IResponse> SetGlobalFieldAsync(string baseTable, string fieldName, string targetValue)
        {
            throw new NotImplementedException();
        }

        public override Task<IResponse> UpdateContainer(string layout, int recordId, string fieldName, string fileName, byte[] content)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
