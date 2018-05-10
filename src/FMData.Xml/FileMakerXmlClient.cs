using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;

namespace FMData.Xml
{
    public class FileMakerXmlClient : IFileMakerApiClient
    {
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
        /// FM Data Constructor. Injects a new plain old <see ref="HttpClient"> instance to the class.
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

        public async Task<IResponse> CreateAsync<T>(T input)
        {
            // setup 
            var layout = "layout";

            var dictionary = input.GetType().GetTypeInfo().DeclaredProperties
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(input, null));

            var url = _fmsUri + "/fmi/xml/fmresultset.xml";

            var stringContent = string.Join("", dictionary.Select(i => $"&{Uri.EscapeUriString(i.Key)}={Uri.EscapeUriString(i.Value.ToString())}"));
            var httpRequestContent = new StringContent($"-db={_fileName}&-lay={layout}{stringContent}");

            var response = await _client.PostAsync(url, httpRequestContent);

            if (response.IsSuccessStatusCode)
            {
                // process response data return OK
                return new BaseResponse { Result = "OK", ErrorCode = "" };
            }

            throw new Exception("Unable to complete request");
            
        }

        public Task<IResponse> CreateAsync<T>(string layout, T input)
        {
            throw new NotImplementedException();
        }

        public Task<IResponse> CreateAsync<T>(ICreateRequest<T> req)
        {
            throw new NotImplementedException();
        }

        public Task<IResponse> DeleteAsync(IDeleteRequest req)
        {
            throw new NotImplementedException();
        }

        public Task<IResponse> DeleteAsync<T>(int recId, T delete)
        {
            throw new NotImplementedException();
        }

        public Task<IResponse> DeleteAsync(int recId, string layout)
        {
            throw new NotImplementedException();
        }

        public Task<IResponse> EditAsync(IEditRequest<Dictionary<string, string>> req)
        {
            throw new NotImplementedException();
        }

        public Task<IResponse> EditAsync<T>(IEditRequest<T> req)
        {
            throw new NotImplementedException();
        }

        public Task<IResponse> EditAsync<T>(int recordId, T input)
        {
            throw new NotImplementedException();
        }

        public Task<IResponse> EditAsync<T>(string layout, int recordId, T input)
        {
            throw new NotImplementedException();
        }

        public Task<IFindResponse<Dictionary<string, string>>> FindAsync(IFindRequest<Dictionary<string, string>> req)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> FindAsync<T>(IFindRequest<Dictionary<string, string>> req)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> FindAsync<T>(IFindRequest<T> req)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> FindAsync<T>(T request)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> FindAsync<T>(string layout, T request)
        {
            throw new NotImplementedException();
        }
    }
}
