namespace FMREST
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public class FmsClient : IFmsClient
    {
        private readonly string _fmsUri;
        private readonly string _fileName;

        private string dataToken;

        public FmsClient(string fmsUri, string file)
        {
            _fmsUri = fmsUri;
            _fileName = file;
        }

        public string AuthEndpoint => $"{_fmsUri}/fmi/rest/api/auth/{_fileName}";
        public string FindEndpoint(string layout) => $"{_fmsUri}/fmi/rest/api/find/{_fileName}/{layout}";

        public async Task<FmsAuthResponse> AuthenticateAsync(string username, string password, string layout)
        {
            using (var client = new HttpClient())
            {
                var str = $"{{ \"user\": \"{username}\", \"password\" : \"{password}\", \"layout\": \"{layout}\" }}";
                var httpContent = new StringContent(str, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(AuthEndpoint, httpContent);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<FmsAuthResponse>(responseJson);
                    this.dataToken = responseObject.Token;
                    return responseObject;
                }
            }

            throw new Exception("Could not authenticate.");
        }


        public async Task<FindResponse> FindAsync()
        {
            var req = new FindRequest();
            req.Query = new List<Dictionary<string, string>>() {
                new Dictionary<string,string>()
                {
                    {"Name","Bross"}
                },
                new Dictionary<string,string>()
                {
                    {"Name","Admin"},{"omit","true"},
                }
            };

            using (var client = new HttpClient())
            {
                var httpContent = new StringContent(req.ToJson(), Encoding.UTF8, "application/json");
                httpContent.Headers.Add("FM-Data-token", this.dataToken);
                var response = await client.PostAsync(FindEndpoint("users"), httpContent);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<FindResponse>(responseJson);
                    return responseObject;
                }
            }

            throw new Exception("Find Rquest Error");
        }

    }
}