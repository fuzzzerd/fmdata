using FMData.Xml.Tests.TestModels;
using RichardSzalay.MockHttp;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FMData.Xml.Tests
{
    public class FindRequestTests
    {
        string server = "http://localhost";
        string file = "test-file";
        string user = "unit";
        string pass = "test";

        [Fact]
        public async Task FindRecord_Should_MatchName()
        {
            // arrange 
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/xml/fmresultset.xml")
                .WithPartialContent("-find")
                .WithPartialContent(Uri.EscapeDataString("Spring in Giverny 3"))
                .Respond(HttpStatusCode.OK, "application/xml", XmlResponses.GrammarSample_fmresultset);

            var fdc = new FileMakerXmlClient(mockHttp.ToHttpClient(), server, file, user, pass);

            var findRequest = new Art()
            {
                Title = "Spring in Giverny 3"
            };

            var response = await fdc.FindAsync(findRequest);

            Assert.NotEmpty(response);
            var first = response.First();
            Assert.Equal("Spring in Giverny 3", first.Title);
        }

        [Fact]
        public async Task FindRecord_Should_MatchLength()
        {
            // arrange 
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/xml/fmresultset.xml")
                .WithPartialContent("-find")
                .WithPartialContent(Uri.EscapeDataString("Spring in Giverny 3"))
                .Respond(HttpStatusCode.OK, "application/xml", XmlResponses.GrammarSample_fmresultset);

            var fdc = new FileMakerXmlClient(mockHttp.ToHttpClient(), server, file, user, pass);

            var findRequest = new Art()
            {
                Title = "Spring in Giverny 3"
            };

            var response = await fdc.FindAsync(findRequest);

            Assert.NotEmpty(response);
            var first = response.First();
            Assert.Equal(19, first.length);
        }

        [Fact]
        public async Task FindRecord_WithPortal_Should_MatchLength()
        {
            // arrange 
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/xml/fmresultset.xml")
                .WithPartialContent("-find")
                .WithPartialContent(Uri.EscapeDataString("Spring in Giverny 3"))
                .Respond(HttpStatusCode.OK, "application/xml", XmlResponses.GrammarSample_fmresultset);

            var fdc = new FileMakerXmlClient(mockHttp.ToHttpClient(), server, file, user, pass);

            var findRequest = new ArtWithPortal()
            {
                Title = "Spring in Giverny 3"
            };

            var response = await fdc.FindAsync(findRequest);

            Assert.NotEmpty(response);
            var first = response.First();
            Assert.Equal(19, first.length);
            Assert.Equal("Chicago", first.Locations.First().Location);
        }
    }
}