using FMData.Xml.Tests.TestModels;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FMData.Xml.Tests
{
    public class DeleteRequestTests
    {
        string server = "http://localhost";
        string file = "test-file";
        string user = "unit";
        string pass = "test";

        [Fact]
        public async Task DeleteRecord_ShouldHaveDelete()
        {
            // arrange 
            var nameToMatch = "-recid=15";

            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/xml/fmresultset.xml")
                .WithPartialContent($"-delete")
                .WithPartialContent(nameToMatch)
                .Respond(HttpStatusCode.OK, "application/xml", "");

            var fdc = new FileMakerXmlClient(mockHttp.ToHttpClient(), server, file, user, pass);

            var response = await fdc.DeleteAsync(15, "layout");

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }
    }
}