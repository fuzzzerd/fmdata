using FMData.Xml.Tests.TestModels;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FMData.Xml.Tests
{
    public class EditRequestTests
    {
        string server = "http://localhost";
        string file = "test-file";
        string user = "unit";
        string pass = "test";

        [Fact]
        public async Task EditRecord_ShouldMatch_SentData()
        {
            // arrange 
            var nameToMatch = "fuzzzerd";

            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/xml/fmresultset.xml")
                .WithPartialContent($"-edit")
                .WithPartialContent(nameToMatch)
                .Respond(HttpStatusCode.OK, "application/xml", "");

            var fdc = new FileMakerXmlClient(mockHttp.ToHttpClient(), server, file, user, pass);

            var mtoEdit = new User()
            {
                Id = 3,
                Name = nameToMatch
            };

            var response = await fdc.EditAsync(5, mtoEdit);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }
    }
}