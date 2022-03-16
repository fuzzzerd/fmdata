using FMData.Rest.Requests;
using FMData.Rest.Tests.TestModels;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Net.Http;

namespace FMData.Rest.Tests
{
    public class FindTestsHelpers
    {
        public static ConnectionInfo Connection => new() { FmsUri = Server, Database = File, Username = User, Password = Pass };
        public static readonly string Server = "http://localhost";
        public static readonly string File = "test-file";
        public static readonly string User = "unit";
        public static readonly string Pass = "test";

        public static IFileMakerApiClient GetMockedFDC()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
               .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Get, $"{Server}/fmi/data/v1/databases/{File}/layout*")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/layout/_find")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/Users/_find")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), new ConnectionInfo { FmsUri = Server, Database = File, Username = User, Password = Pass });
            return fdc;
        }

        public static IFindRequest<Dictionary<string, string>> FindReq()
        {
            var r = new FindRequest<Dictionary<string, string>>() { Layout = "layout" };

            r.AddQuery(new Dictionary<string, string>()
            {
                {"Name","fuzzzerd"}
            }, false);

            r.AddQuery(
            new Dictionary<string, string>()
            {
                {"Name","Admin"}, {"omit","true"},
            }, false);

            return r;
        }

        public static IFindRequest<User> FindUserReqWithLayoutOverride()
        {
            var r = new FindRequest<User>() { Layout = "layout" };
            r.AddQuery(new User() { Id = 1 }, false);
            return r;
        }
    }
}
