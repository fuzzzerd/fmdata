using FMData.Xml.Tests.TestModels;
using RichardSzalay.MockHttp;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FMData.Xml.Tests
{
    public class CreateRequestTests
    {
        string server = "http://localhost";
        string file = "test-file";
        string user = "unit";
        string pass = "test";

        [Fact]
        public async Task CreateRecord_ShouldMatch_SentData()
        {
            // arrange 
            var nameToMatch = "fuzzzerd";

            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When($"{server}/fmi/xml/fmresultset.xml")
                .WithPartialContent($"-new")
                .WithPartialContent($"-db={file}")
                .WithPartialContent(nameToMatch)
                .Respond(HttpStatusCode.OK, "application/json", "");

            var fdc = new FileMakerXmlClient(mockHttp.ToHttpClient(), server, file, user, pass);

            var mtoCreate = new User()
            {
                Id = 3,
                Name = nameToMatch
            };

            var response = await fdc.CreateAsync(mtoCreate);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }
    }
}