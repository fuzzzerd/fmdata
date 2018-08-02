﻿using FMData.Xml.Requests;
using FMData.Xml.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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


        protected override ICreateRequest<T> _createFactory<T>() => new CreateRequest<T>();

        protected override IEditRequest<T> _editFactory<T>()
        {
            throw new NotImplementedException();
        }

        protected override IFindRequest<T> _findFactory<T>() => new FindRequest<T>();

        protected override IDeleteRequest _deleteFactory()
        {
            throw new NotImplementedException();
        }


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
            : this(new HttpClient(new HttpClientHandler { Credentials = new NetworkCredential(user, pass) }), fmsUri, file, user, pass, initialLayout) { }

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
            _fileName = Uri.EscapeDataString(file);
            _userName = Uri.EscapeDataString(user);
            _password = Uri.EscapeDataString(pass);
        }
        #endregion

        #region Special Implementations
        public override Task<T> GetByFileMakerIdAsync<T>(string layout, int fileMakerId, Func<T, int, object> fmId = null)
        {
            throw new NotImplementedException();
        }

        public override Task<IFindResponse<Dictionary<string, string>>> SendAsync(IFindRequest<Dictionary<string, string>> req)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<T>> FindAsync<T>(string layout, Dictionary<string, string> req)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region SendAsync Implementations
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="req"></param>
        /// <returns></returns>
        public async override Task<ICreateResponse> SendAsync<T>(ICreateRequest<T> req)
        {
            // setup 
            var layout = req.Layout;

            var dictionary = req.Data.GetType().GetTypeInfo().DeclaredProperties
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(req.Data, null));

            var url = _fmsUri + "/fmi/xml/fmresultset.xml";

            var stringContent = string.Join("", dictionary.Select(i => $"&{Uri.EscapeDataString(i.Key)}={Uri.EscapeDataString(i.Value.ToString())}"));
            var httpRequestContent = new StringContent($"-new&-db={_fileName}&-lay={layout}{stringContent}");

            var response = await _client.PostAsync(url, httpRequestContent);

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

        public override Task<IResponse> SendAsync(IDeleteRequest req)
        {
            throw new NotImplementedException();
        }

        public override Task<IEditResponse> SendAsync<T>(IEditRequest<T> req)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="req"></param>
        /// <param name="fmId"></param>
        /// <returns></returns>
        public override async Task<IEnumerable<T>> SendAsync<T>(IFindRequest<T> req, Func<T, int, object> fmId = null)
        {
            var url = _fmsUri + "/fmi/xml/fmresultset.xml";

            var dictionary = req.Query.First().AsDictionary(false);

            var stringContent = string.Join("", dictionary.Select(i => $"&{Uri.EscapeDataString(i.Key)}={Uri.EscapeDataString(i.Value.ToString())}"));
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
                    .Select(r => new RecordBase<T, T>
                    {
                        RecordId = Convert.ToInt32(r.Attribute("record-id").Value),
                        ModId = Convert.ToInt32(r.Attribute("mod-id").Value),
                        FieldData = r.Elements(_ns + "field")
                            .ToDictionary(
                                k => k.Attribute("name").Value,
                                v => v.Attribute("name").Value == "length" ? Convert.ChangeType(v.Value, typeof(int)) : v.Value
                            ).ToObject<T>()
                    });

                return records.Select(r => r.FieldData);
            }

            return null;
        }
        #endregion


        public override Task<IResponse> SetGlobalFieldAsync(string baseTable, string fieldName, string targetValue)
        {
            throw new NotImplementedException();
        }

        public override Task<IEditResponse> UpdateContainerAsync(string layout, int recordId, string fieldName, string fileName, int repetition, byte[] content)
        {
            throw new NotImplementedException();
        }

        #region Private Helpers and utility methods

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