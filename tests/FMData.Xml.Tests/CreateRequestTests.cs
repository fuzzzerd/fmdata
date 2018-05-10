using System;
using System.Collections.Generic;
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
        private static IFileMakerApiClient GetMockedFDC()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "layout";

            mockHttp.When($"{server}/fmi/xml/fmresultset.xml")
                .Respond(HttpStatusCode.OK, "application/json", "");

            //mockHttp.When($"{server}/fmi/rest/api/record/{file}/{layout}")
            //    .WithPartialContent("data") // make sure that the body content contains the 'data' object expected by fms
            //    .Respond("application/json", DataApiResponses.SuccessfulCreate());

            var fdc = new FileMakerXmlClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);
            return fdc;
        }

        [Fact]
        public async Task CreateShould_ReturnRecordId()
        {
            IFileMakerApiClient fdc = GetMockedFDC();

            var mtoCreate = new User()
            {
                Id = 3,
                Name = "fuzzzerd"
            };

            var response = await fdc.CreateAsync(mtoCreate);

            Assert.NotNull(response);
            Assert.Equal("OK", response.Result);
        }
    }
}