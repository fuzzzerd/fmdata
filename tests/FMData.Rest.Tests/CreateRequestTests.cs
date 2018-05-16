using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FMData;
using FMData.Rest;
using FMData.Rest.Requests;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Tests
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

            mockHttp.When($"{server}/fmi/rest/api/auth/{file}")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When($"{server}/fmi/rest/api/record/{file}/{layout}")
                .WithPartialContent("data") // make sure that the body content contains the 'data' object expected by fms
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);
            return fdc;
        }

        [Fact]
        public async Task CreateShould_ReturnRecordId()
        {
            IFileMakerApiClient fdc = GetMockedFDC();

            var req = new CreateRequest<Dictionary<string, string>>()
            {
                Layout = "layout",
                Data = new Dictionary<string, string>()
                {
                    { "Name", "Fuzzerd" },
                    { "AnotherField", "Another Valuee" }
                }
            };

            // requires cast to call correct method -- maybe needs renamed since overloading isn't working out so well
            var response = await fdc.SendAsync(req);

            Assert.NotNull(response);
            Assert.Equal("OK", response.Result);
        }

        [Fact]
        public async Task Create_WithoutLayout_ThrowsArgumentException()
        {
            IFileMakerApiClient fdc = GetMockedFDC();

            var req = new CreateRequest<Dictionary<string, string>>()
            {
                Data = new Dictionary<string, string>()
                {
                    { "Name", "Fuzzerd" },
                    { "AnotherField", "Another Valuee" }
                }
            };

            await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.CreateAsync(req));
        }
    }
}