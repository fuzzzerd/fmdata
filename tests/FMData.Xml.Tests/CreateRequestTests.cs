using System.Net;
using System.Threading.Tasks;
using FMData.Xml.Tests.TestModels;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Xml.Tests
{
    public class CreateRequestTests
    {
        readonly string _server = "http://localhost";
        readonly string _file = "test-file";
        readonly string _user = "unit";
        readonly string _pass = "test";

        [Fact]
        public async Task CreateRecord_ShouldMatch_SentData()
        {
            // arrange 
            var nameToMatch = "fuzzzerd";

            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When($"{_server}/fmi/xml/fmresultset.xml")
                .WithPartialContent($"-new")
                .WithPartialContent($"-db={_file}")
                .WithPartialContent(nameToMatch)
                .Respond(HttpStatusCode.OK, "application/json", "");

            var fdc = new FileMakerXmlClient(mockHttp.ToHttpClient(), _server, _file, _user, _pass);

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
