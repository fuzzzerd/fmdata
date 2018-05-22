using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FMData.Rest;
using FMData.Rest.Requests;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Tests
{
    public class EditRequestTests
    {
        [Fact]
        public async Task EditShould_UpdateRecord_WithId()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "layout";

            mockHttp.When($"{server}/fmi/data/v1/databases/{file}/sessions")
               .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Put, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records*")
                .WithPartialContent("fieldData")
                .Respond("application/json", DataApiResponses.SuccessfulEdit());

            IEnumerable<int> x = new List<int>();

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);

            var req = new EditRequest<Dictionary<string, string>>()
            {
                Layout = "layout",
                RecordId = "264",
                Data = new Dictionary<string, string>()
                    {
                        { "Name", "Fuzzerd-Updated" },
                        { "AnotherField", "Another-Updated" }
                    }
            };
            var response = await fdc.SendAsync(req);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");

        }
    }
}