using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FMData.Rest.Tests.TestModels;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Rest.Tests
{
    public class FindAsyncMapperTests
    {
        private static readonly string Server = "http://localhost";
        private static readonly string File = "test-file";
        private static readonly string User = "unit";
        private static readonly string Pass = "test";
        private static readonly string Layout = "Users";

        private static FileMakerRestClient GetClient(MockHttpMessageHandler mockHttp)
        {
            return new FileMakerRestClient(
                mockHttp.ToHttpClient(),
                new ConnectionInfo
                {
                    FmsUri = Server,
                    Database = File,
                    Username = User,
                    Password = Pass
                });
        }

        private static MockHttpMessageHandler CreateMockWithFindAndAuth()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Delete, $"{Server}/fmi/data/v1/databases/{File}/sessions*")
                .Respond(HttpStatusCode.OK, "application/json", "");

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/_find")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            return mockHttp;
        }

        [Fact(DisplayName = "FindAsync with fmIdFunc maps FileMaker RecordId")]
        public async Task FindAsync_WithFmIdFunc_MapsRecordId()
        {
            var mockHttp = CreateMockWithFindAndAuth();
            using var client = GetClient(mockHttp);

            var results = await client.FindAsync(
                new User { Name = "fuzzzerd" },
                (o, id) => o.FileMakerRecordId = id);

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.FileMakerRecordId == 4);
            Assert.Contains(results, r => r.FileMakerRecordId == 1);
        }

        [Fact(DisplayName = "FindAsync with skip/take and fmIdFunc maps RecordId")]
        public async Task FindAsync_WithSkipTakeAndFmIdFunc_MapsRecordId()
        {
            var mockHttp = CreateMockWithFindAndAuth();
            using var client = GetClient(mockHttp);

            var results = await client.FindAsync(
                new User { Name = "fuzzzerd" },
                skip: 0,
                take: 10,
                fmIdFunc: (o, id) => o.FileMakerRecordId = id);

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.FileMakerRecordId == 4);
        }

        [Fact(DisplayName = "FindAsync with script and fmIdFunc maps RecordId")]
        public async Task FindAsync_WithScriptAndFmIdFunc_MapsRecordId()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Delete, $"{Server}/fmi/data/v1/databases/{File}/sessions*")
                .Respond(HttpStatusCode.OK, "application/json", "");

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/_find")
                .WithPartialContent("script")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            using var client = GetClient(mockHttp);

            var results = await client.FindAsync(
                new User { Name = "fuzzzerd" },
                script: "TestScript",
                scriptParameter: "param",
                fmIdFunc: (o, id) => o.FileMakerRecordId = id);

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.FileMakerRecordId == 4);
        }

        [Fact(DisplayName = "FindAsync with skip/take, script, and fmIdFunc maps RecordId")]
        public async Task FindAsync_WithSkipTakeScriptAndFmIdFunc_MapsRecordId()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Delete, $"{Server}/fmi/data/v1/databases/{File}/sessions*")
                .Respond(HttpStatusCode.OK, "application/json", "");

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/_find")
                .WithPartialContent("limit")
                .WithPartialContent("script")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            using var client = GetClient(mockHttp);

            var results = await client.FindAsync(
                new User { Name = "fuzzzerd" },
                skip: 0,
                take: 5,
                script: "TestScript",
                scriptParameter: "param",
                fmIdFunc: (o, id) => o.FileMakerRecordId = id);

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.FileMakerRecordId == 4);
        }

        [Fact(DisplayName = "FindAsync with both fmIdFunc and fmModIdFunc maps both IDs")]
        public async Task FindAsync_WithBothMappers_MapsBothIds()
        {
            var mockHttp = CreateMockWithFindAndAuth();
            using var client = GetClient(mockHttp);

            var results = await client.FindAsync(
                new User { Name = "fuzzzerd" },
                skip: 0,
                take: 10,
                script: null,
                scriptParameter: null,
                fmIdFunc: (o, id) => o.FileMakerRecordId = id,
                fmModIdFunc: (o, modId) => o.FileMakerModId = modId);

            Assert.NotEmpty(results);
            var first = results.First(r => r.FileMakerRecordId == 4);
            Assert.Equal(4, first.FileMakerRecordId);
            Assert.Equal(0, first.FileMakerModId);

            var second = results.First(r => r.FileMakerRecordId == 1);
            Assert.Equal(1, second.FileMakerRecordId);
            Assert.Equal(12, second.FileMakerModId);
        }

        [Fact(DisplayName = "FindAsync with layout override uses specified layout")]
        public async Task FindAsync_WithLayoutOverride_UsesSpecifiedLayout()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Delete, $"{Server}/fmi/data/v1/databases/{File}/sessions*")
                .Respond(HttpStatusCode.OK, "application/json", "");

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/CustomLayout/_find")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            using var client = GetClient(mockHttp);

            var results = await client.FindAsync("CustomLayout", new User { Name = "fuzzzerd" });

            Assert.NotEmpty(results);
        }
    }
}
