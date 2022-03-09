using FMData.Rest.Tests.TestModels;
using RichardSzalay.MockHttp;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FMData.Rest.Tests
{
    public class EditAsyncTests
    {
        [Fact(DisplayName = "Script In Edit Should Show In JSON")]
        public async Task Edit_WithScript_ShouldHaveScript()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "Users";
            var rid = 25;

            mockHttp.When($"{server}/fmi/data/v1/databases/{file}/sessions")
               .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(new HttpMethod("PATCH"), $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records/{rid}")
                .WithPartialContent("script").WithPartialContent("myscr_name")
                .Respond("application/json", DataApiResponses.SuccessfulEdit());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), new ConnectionInfo { FmsUri = server, Database = file, Username = user, Password = pass });

            var response = await fdc.EditAsync(rid, "myscr_name", null, new User() { Name = "test user" });

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }
    }
}
