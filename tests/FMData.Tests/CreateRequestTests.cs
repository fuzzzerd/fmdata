using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FMData.Requests;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Tests
{
    public class CreateRequestTests
    {
        private static FMDataClient GetMockedFDC()
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

            var fdc = new FMDataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);
            return fdc;
        }

        [Fact]
        public async Task CreateShould_ReturnRecordId()
        {
            FMDataClient fdc = GetMockedFDC();

            var req = new CreateRequest<Dictionary<string, string>>()
            {
                Layout = "layout",
                Data = new Dictionary<string, string>()
                {
                    { "Name", "Fuzzerd" },
                    { "AnotherField", "Another Valuee" }
                }
            };
            var response = await fdc.ExecuteCreateAsync(req);

            Assert.NotNull(response);
            Assert.NotNull(response.RecordId);
            Assert.True(response.RecordId != "0");
        }

        [Fact]
        public async Task Create_WithoutLayout_ThrowsArgumentException()
        {
            FMDataClient fdc = GetMockedFDC();

            var req = new CreateRequest<Dictionary<string, string>>()
            {
                Data = new Dictionary<string, string>()
                {
                    { "Name", "Fuzzerd" },
                    { "AnotherField", "Another Valuee" }
                }
            };

            await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.ExecuteCreateAsync(req));
        }
    }
}