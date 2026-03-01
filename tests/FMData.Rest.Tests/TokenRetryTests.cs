using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FMData.Rest.Requests;
using FMData.Rest.Tests.TestModels;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Rest.Tests
{
    public class TokenRetryTests
    {
        private const string Server = "http://localhost";
        private const string File = "test-file";
        private const string User = "unit";
        private const string Pass = "test";
        private const string Layout = "layout";

        [Fact(DisplayName = "FindAsync Should Retry On 401 Unauthorized")]
        public async Task FindAsync_ShouldRetry_OnUnauthorized()
        {
            var mockHttp = new MockHttpMessageHandler();

            // 1. Initial auth succeeds
            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            // 2. Find request returns 401
            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/_find")
                .Respond(HttpStatusCode.Unauthorized, "application/json", DataApiResponses.Authentication401());

            // 3. Re-auth succeeds
            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            // 4. Retry find succeeds
            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/_find")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            using var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(),
                new ConnectionInfo { FmsUri = Server, Database = File, Username = User, Password = Pass });

#pragma warning disable CS0618 // intentionally testing the obsolete overload
            var response = await fdc.FindAsync<User>(Layout, new Dictionary<string, string> { { "Name", "test" } });
#pragma warning restore CS0618

            Assert.NotNull(response);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact(DisplayName = "CreateAsync Should Retry On 401 Unauthorized")]
        public async Task CreateAsync_ShouldRetry_OnUnauthorized()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records")
                .Respond(HttpStatusCode.Unauthorized, "application/json", DataApiResponses.Authentication401());

            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records")
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

            using var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(),
                new ConnectionInfo { FmsUri = Server, Database = File, Username = User, Password = Pass });

            var req = new CreateRequest<Dictionary<string, string>>
            {
                Layout = Layout,
                Data = new Dictionary<string, string> { { "Name", "Test" } }
            };
            var response = await fdc.SendAsync(req);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact(DisplayName = "EditAsync Should Retry On 401 Unauthorized")]
        public async Task EditAsync_ShouldRetry_OnUnauthorized()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.Expect(new HttpMethod("PATCH"), $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records/264")
                .Respond(HttpStatusCode.Unauthorized, "application/json", DataApiResponses.Authentication401());

            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.Expect(new HttpMethod("PATCH"), $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records/264")
                .Respond("application/json", DataApiResponses.SuccessfulEdit());

            using var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(),
                new ConnectionInfo { FmsUri = Server, Database = File, Username = User, Password = Pass });

            var req = new EditRequest<Dictionary<string, string>>
            {
                Layout = Layout,
                RecordId = 264,
                Data = new Dictionary<string, string> { { "Name", "Updated" } }
            };
            var response = await fdc.SendAsync(req);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact(DisplayName = "DeleteAsync Should Retry On 401 Unauthorized")]
        public async Task DeleteAsync_ShouldRetry_OnUnauthorized()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.Expect(HttpMethod.Delete, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records/1234")
                .Respond(HttpStatusCode.Unauthorized, "application/json", DataApiResponses.Authentication401());

            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.Expect(HttpMethod.Delete, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records/1234")
                .Respond("application/json", DataApiResponses.SuccessfulDelete());

            using var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(),
                new ConnectionInfo { FmsUri = Server, Database = File, Username = User, Password = Pass });

            var req = new DeleteRequest { Layout = Layout, RecordId = 1234 };
            var response = await fdc.SendAsync(req);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact(DisplayName = "RunScriptAsync Should Retry On 401 Unauthorized")]
        public async Task RunScriptAsync_ShouldRetry_OnUnauthorized()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.Expect(HttpMethod.Get, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/script/TestScript")
                .Respond(HttpStatusCode.Unauthorized, "application/json", DataApiResponses.Authentication401());

            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var scriptResponse = System.IO.File.ReadAllText(Path.Combine("ResponseData", "ScriptResponseOK.json"));
            mockHttp.Expect(HttpMethod.Get, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/script/TestScript")
                .Respond("application/json", scriptResponse);

            using var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(),
                new ConnectionInfo { FmsUri = Server, Database = File, Username = User, Password = Pass });

            var response = await fdc.RunScriptAsync(Layout, "TestScript", null);

            Assert.NotNull(response);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact(DisplayName = "GetLayoutsAsync Should Retry On 401 Unauthorized")]
        public async Task GetLayoutsAsync_ShouldRetry_OnUnauthorized()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.Expect(HttpMethod.Get, $"{Server}/fmi/data/v1/databases/{File}/layouts")
                .Respond(HttpStatusCode.Unauthorized, "application/json", DataApiResponses.Authentication401());

            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var layoutData = System.IO.File.ReadAllText(Path.Combine("ResponseData", "LayoutList.json"));
            mockHttp.Expect(HttpMethod.Get, $"{Server}/fmi/data/v1/databases/{File}/layouts")
                .Respond("application/json", layoutData);

            using var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(),
                new ConnectionInfo { FmsUri = Server, Database = File, Username = User, Password = Pass });

            var response = await fdc.GetLayoutsAsync();

            Assert.NotNull(response);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact(DisplayName = "UpdateContainerAsync Should Retry On 401 Unauthorized")]
        public async Task UpdateContainerAsync_ShouldRetry_OnUnauthorized()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records/12/containers/field/1")
                .Respond(HttpStatusCode.Unauthorized, "application/json", DataApiResponses.Authentication401());

            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.Expect(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records/12/containers/field/1")
                .Respond("application/json", DataApiResponses.SuccessfulEdit());

            using var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(),
                new ConnectionInfo { FmsUri = Server, Database = File, Username = User, Password = Pass });

            var bytes = new byte[] { 0x01, 0x02, 0x03 };
            var response = await fdc.UpdateContainerAsync(Layout, 12, "field", "test.jpg", 1, bytes);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}
