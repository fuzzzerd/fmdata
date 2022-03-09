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
        readonly string _server = "http://localhost";
        readonly string _file = "test-file";
        readonly string _user = "unit";
        readonly string _pass = "test";

        [Fact]
        public async Task DeleteRecord_ShouldHaveDelete()
        {
            // arrange 
            var nameToMatch = "-recid=15";

            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{_server}/fmi/xml/fmresultset.xml")
                .WithPartialContent($"-delete")
                .WithPartialContent(nameToMatch)
                .Respond(HttpStatusCode.OK, "application/xml", "");

            var fdc = new FileMakerXmlClient(mockHttp.ToHttpClient(), _server, _file, _user, _pass);

            var response = await fdc.DeleteAsync(15, "layout");

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }
    }
}
