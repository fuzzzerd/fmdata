using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FMData;
using FMData.Xml;
using FMData.Xml.Tests.TestModels;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Xml.Tests
{
    public class CreateRequestTests
    {
        string server = "http://localhost";
        string file = "test-file";
        string user = "unit";
        string pass = "test";
        string layout = "layout";

        [Fact]
        public async Task CreateRecord_ShouldMatch_SentData()
        {
            // arrange 
            var nameToMatch = "fuzzzerd";

            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When($"{server}/fmi/xml/fmresultset.xml")
                .WithPartialContent($"-new")
                .WithPartialContent(nameToMatch)
                .Respond(HttpStatusCode.OK, "application/json", "");

            var fdc = new FileMakerXmlClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);

            var mtoCreate = new User()
            {
                Id = 3,
                Name = nameToMatch
            };

            var response = await fdc.CreateAsync(mtoCreate);

            Assert.NotNull(response);
            Assert.Equal("OK", response.Result);
        }
    }
}