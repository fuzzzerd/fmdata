using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FMData.Rest.Requests;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Rest.Tests
{
    public class EditRequestTests
    {
        private static readonly string s_server = "http://localhost";
        private static readonly string s_file = "test-file";
        private static readonly string s_user = "unit";
        private static readonly string s_pass = "test";
        private static readonly string s_layout = "layout";

        private static FileMakerRestClient GenerateClient()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When($"{s_server}/fmi/data/v1/databases/{s_file}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(new HttpMethod("PATCH"), $"{s_server}/fmi/data/v1/databases/{s_file}/layouts/{s_layout}/records*")
                .WithPartialContent("fieldData")
                .Respond("application/json", DataApiResponses.SuccessfulEdit());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), new ConnectionInfo { FmsUri = s_server, Database = s_file, Username = s_user, Password = s_pass });
            return fdc;
        }

        [Fact(DisplayName = "With ID Edit Should Succeed")]
        public async Task EditShould_UpdateRecord_WithId()
        {
            var fdc = GenerateClient();

            var req = new EditRequest<Dictionary<string, string>>()
            {
                Layout = "layout",
                RecordId = 264,
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

        [Fact(DisplayName = "New ModId Should Be Generated")]
        public async Task Edit_ShouldReturn_NewModId()
        {
            var fdc = GenerateClient();

            var req = new EditRequest<Dictionary<string, string>>()
            {
                Layout = "layout",
                RecordId = 264,
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

        [Fact]
        public async Task EditAsync_Should_Throw_FMDataException_For_InternalServerError()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(new HttpMethod("PATCH"), $"{s_server}/fmi/data/v1/databases/{s_file}/layouts/{s_layout}/records*")
                    .WithPartialContent("fieldData")
                    .Respond(HttpStatusCode.InternalServerError, "application/json", DataApiResponses.FieldNotFound());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var req = new EditRequest<Dictionary<string, string>>()
            {
                Layout = s_layout,
                RecordId = 264,
                Data = new Dictionary<string, string>()
                    {
                        { "Name", "Fuzzerd-Updated" },
                        { "AnotherField", "Another-Updated" }
                    }
            };

            // act
            // assert
            await Assert.ThrowsAsync<FMDataException>(async () => await fdc.SendAsync(req));
        }
    }
}
