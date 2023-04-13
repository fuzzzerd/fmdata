using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Rest.Tests
{
    public class ScriptTests
    {
        [Fact(DisplayName = "Get Scripts Should Return Script List")]
        public async Task GetScripts_Should_Return_Script_List()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var layoutData = System.IO.File.ReadAllText(Path.Combine("ResponseData", "ScriptList.json"));
            mockHttp.When($"{server}/fmi/data/v1/databases/{file}/scripts")
                    .Respond("application/json", layoutData);

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), new ConnectionInfo { FmsUri = server, Database = file, Username = user, Password = pass });

            var response = await fdc.GetScriptsAsync();

            Assert.NotNull(response);
            Assert.Equal("GotoFirst", response.First().Name);
        }

        [Fact(DisplayName = "No Script Error Should Return Script Result")]
        public async Task Run_Script_Should_Return_Script_Result_When_ScriptError_Zero()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "layout";

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var scriptResponse = System.IO.File.ReadAllText(Path.Combine("ResponseData", "ScriptResponseOK.json"));
            mockHttp.When($"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/script/*")
                    .Respond("application/json", scriptResponse);

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), new ConnectionInfo { FmsUri = server, Database = file, Username = user, Password = pass });

            var response = await fdc.RunScriptAsync(layout, "script-name", null);

            Assert.Equal("Text Based Script Result", response);
        }
    }
}
