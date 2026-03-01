using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FMData.Rest.Requests;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Rest.Tests
{
    public class ExecuteRequestAsyncTests
    {
        private static readonly string Server = "http://localhost";
        private static readonly string File = "test-file";
        private static readonly string Layout = "layout";

        private static ConnectionInfo TestConnection => new ConnectionInfo
        {
            FmsUri = Server,
            Database = File,
            Username = "unit",
            Password = "test"
        };

        private static MockHttpMessageHandler CreateMock()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Delete, $"{Server}/fmi/data/v1/databases/{File}/sessions*")
                .Respond(HttpStatusCode.OK, "application/json", "");

            return mockHttp;
        }

        #region GetRecordsEndpoint

        [Fact(DisplayName = "GetRecordsEndpoint produces correct URL")]
        public void GetRecordsEndpoint_ProducesCorrectUrl()
        {
            var mockHttp = new MockHttpMessageHandler();
            using var client = new FileMakerRestClient(mockHttp.ToHttpClient(), TestConnection);

            var endpoint = client.GetRecordsEndpoint(Layout, 10, 5);

            Assert.Equal($"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records?_limit=10&_offset=5", endpoint);
        }

        [Fact(DisplayName = "GetRecordsEndpoint escapes layout name")]
        public void GetRecordsEndpoint_EscapesLayoutName()
        {
            var mockHttp = new MockHttpMessageHandler();
            using var client = new FileMakerRestClient(mockHttp.ToHttpClient(), TestConnection);

            var endpoint = client.GetRecordsEndpoint("My Layout", 10, 0);

            Assert.Contains("My%20Layout", endpoint);
        }

        #endregion

        #region ExecuteRequestAsync Typed Overloads

        [Fact(DisplayName = "ExecuteRequestAsync with CreateRequest sends POST")]
        public async Task ExecuteRequestAsync_CreateRequest_SendsPost()
        {
            var mockHttp = CreateMock();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records")
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

            using var client = new FileMakerRestClient(mockHttp.ToHttpClient(), TestConnection);

            var req = new CreateRequest<TestModels.User>
            {
                Layout = Layout,
                Data = new TestModels.User { Name = "test" }
            };

            var response = await client.ExecuteRequestAsync(req);

            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact(DisplayName = "ExecuteRequestAsync with FindRequest sends POST to _find")]
        public async Task ExecuteRequestAsync_FindRequest_SendsPostToFind()
        {
            var mockHttp = CreateMock();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/_find")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            using var client = new FileMakerRestClient(mockHttp.ToHttpClient(), TestConnection);

            var req = new FindRequest<TestModels.User> { Layout = Layout };
            req.AddQuery(new TestModels.User { Name = "test" }, false);

            var response = await client.ExecuteRequestAsync(req);

            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact(DisplayName = "ExecuteRequestAsync with EditRequest sends PATCH")]
        public async Task ExecuteRequestAsync_EditRequest_SendsPatch()
        {
            var mockHttp = CreateMock();

            mockHttp.When(new HttpMethod("PATCH"), $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records/42")
                .Respond("application/json", DataApiResponses.SuccessfulEdit());

            using var client = new FileMakerRestClient(mockHttp.ToHttpClient(), TestConnection);

            var req = new EditRequest<TestModels.User>
            {
                Layout = Layout,
                RecordId = 42,
                Data = new TestModels.User { Name = "updated" }
            };

            var response = await client.ExecuteRequestAsync(req);

            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact(DisplayName = "ExecuteRequestAsync with DeleteRequest sends DELETE")]
        public async Task ExecuteRequestAsync_DeleteRequest_SendsDelete()
        {
            var mockHttp = CreateMock();

            mockHttp.When(HttpMethod.Delete, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records/99")
                .Respond("application/json", DataApiResponses.SuccessfulDelete());

            using var client = new FileMakerRestClient(mockHttp.ToHttpClient(), TestConnection);

            var req = new DeleteRequest { Layout = Layout, RecordId = 99 };

            var response = await client.ExecuteRequestAsync(req);

            Assert.True(response.IsSuccessStatusCode);
        }

        #endregion

        #region ExecuteRequestAsync Core Method

        [Fact(DisplayName = "ExecuteRequestAsync refreshes token before sending")]
        public async Task ExecuteRequestAsync_RefreshesTokenBeforeSending()
        {
            var mockHttp = CreateMock();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/_find")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            using var client = new FileMakerRestClient(mockHttp.ToHttpClient(), TestConnection);

            Assert.False(client.IsAuthenticated);

            var req = new FindRequest<TestModels.User> { Layout = Layout };
            req.AddQuery(new TestModels.User { Name = "test" }, false);

            var response = await client.ExecuteRequestAsync(req);

            Assert.True(client.IsAuthenticated);
            Assert.True(response.IsSuccessStatusCode);
        }

        #endregion
    }
}
