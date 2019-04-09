using FMData.Rest.Requests;
using FMData.Rest.Tests.TestModels;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Net.Http;

namespace FMData.Rest.Tests
{
    public class FindTestsHelpers
    {
        public static ConnectionInfo Connection => new ConnectionInfo { FmsUri = server, Database = file, Username = user, Password = pass };
        public static readonly string server = "http://localhost";
        public static readonly string file = "test-file";
        public static readonly string user = "unit";
        public static readonly string pass = "test";

        public static IFileMakerApiClient GetMockedFDC()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
               .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Get, $"{server}/fmi/data/v1/databases/{file}/layout*")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/layout/_find")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/Users/_find")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), new ConnectionInfo { FmsUri = server, Database = file, Username = user, Password = pass });
            return fdc;
        }

        public static IFindRequest<Dictionary<string, string>> FindReq ()
        {
            var r = new FindRequest<Dictionary<string, string>>() {Layout = "layout"};

            r.AddQuery(new Dictionary<string,string>()
            {
                {"Name","fuzzzerd"}
            }, false);

            r.AddQuery(
            new Dictionary<string,string>()
            {
                {"Name","Admin"}, {"omit","true"},
            }, false);

            return r;
        }

        public static IFindRequest<User> FindUserReqWithLayoutOverride() 
        {
            var r = new FindRequest<User>() { Layout = "layout" };
            r.AddQuery(new User() { Id =1 }, false);
            return r;
        }
    }
}