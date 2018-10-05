using FMData.Rest.Requests;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FMData.Rest.Tests
{
    public class EditRequestTests
    {
        private static readonly string server = "http://localhost";
        private static readonly string file = "test-file";
        private static readonly string user = "unit";
        private static readonly string pass = "test";
        private static readonly string layout = "layout";

        private static FileMakerRestClient GenerateClient()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When($"{server}/fmi/data/v1/databases/{file}/sessions")
               .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(new HttpMethod("PATCH"), $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records*")
                .WithPartialContent("fieldData")
                .Respond("application/json", DataApiResponses.SuccessfulEdit());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);
            return fdc;
        }


        [Fact(DisplayName ="With ID Edit Should Succeed")]
        public async Task EditShould_UpdateRecord_WithId()
        {
            FileMakerRestClient fdc = GenerateClient();

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

        [Fact(DisplayName ="New ModId Should Be Generated")]
        public async Task Edit_ShouldReturn_NewModId()
        {
            FileMakerRestClient fdc = GenerateClient();

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
            Assert.Equal(3, response.Response.ModId);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }
    }
}