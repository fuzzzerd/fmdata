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
        protected override IEditRequest<T> _editFactory<T>()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Factory to get a new Find Request of the correct type.
        /// </summary>
        protected override IFindRequest<T> _findFactory<T>() => new FindRequest<T>();
        /// <summary>
        /// Factory to get a new Delete Request of the correct type.
        /// </summary>
        protected override IDeleteRequest _deleteFactory()
        {
            throw new NotImplementedException();
        }
        #endregion

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
        public FileMakerXmlClient(string fmsUri, string file, string user, string pass)
            : this(new HttpClient(new HttpClientHandler { Credentials = new NetworkCredential(user, pass) }), fmsUri, file, user, pass) { }

        /// <summary>
        /// FM Data Constructor. Injects a new plain old <see ref="HttpClient"/> instance to the class.
        /// </summary>
        /// <param name="client">An <see ref="HttpClient"/> instance to utilize for the liftime of this Data Client.</param>
        /// <param name="fmsUri">FileMaker Server HTTP Uri Endpoint.</param>
        /// <param name="file">Name of the FileMaker Database to connect to.</param>
        /// <param name="user">Account to connect with.</param>
        /// <param name="pass">Account to connect with.</param>
        /// <param name="initialLayout">Layout to use for the initial authentication request.</param>
        public FileMakerXmlClient(HttpClient client, string fmsUri, string file, string user, string pass)
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
        /// Executes a Create Record Request
        /// </summary>
        /// <typeparam name="T">The projected type to be created.</typeparam>
        /// <param name="req">The request record command.</param>
        /// <returns>A response containing the results of the operation.</returns>
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
        /// Executes a Find Request and returns the matching objects projected by the type parameter.
        /// </summary>
        /// <typeparam name="T">The type to project the results against.</typeparam>
        /// <param name="req">The Find Request Command.</param>
        /// <param name="fmId">The function to map FileMaker Record Ids to an object.</param>
        /// <returns>The projected results matching the find request.</returns>
        public override async Task<IEnumerable<T>> SendAsync<T>(IFindRequest<T> req, Func<T, int, object> fmId = null)
        {
            var url = _fmsUri + "/fmi/xml/fmresultset.xml";

            var dictionary = req.Query.First().AsDictionary(false);

            var stringContent = string.Join("", dictionary.Select(i => $"&{Uri.EscapeDataString(i.Key)}={Uri.EscapeDataString(i.Value.ToString())}"));
            var httpRequestContent = new StringContent($"-find&-db={_fileName}&-lay={req.Layout}{stringContent}");

            var response = await _client.PostAsync(url, httpRequestContent);

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
                                v => {
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
                                            return (object)v.Value;
                                    }
                                }
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