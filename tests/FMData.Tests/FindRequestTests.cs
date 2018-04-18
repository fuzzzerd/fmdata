using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FMData.Requests;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Tests
{
    public class FindRequestTests
    {
        private static FMDataClient GetMockedFDC()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "layout";

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/rest/api/auth/{file}")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());


            mockHttp.When(HttpMethod.Post, $"{server}/fmi/rest/api/find/{file}/*")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FMDataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);
            return fdc;
        }

        private FindRequest FindReq => new FindRequest()
        {
            Query = new List<Dictionary<string, string>>()
            {
                new Dictionary<string,string>()
                {
                    {"Name","fuzzzerd"}
                },
                new Dictionary<string,string>()
                {
                    {"Name","Admin"}, {"omit","true"},
                }
            },
            Layout = "layout"
        };

        [Fact]
        public async Task FindShould_ReturnData()
        {
            var fdc = GetMockedFDC();

            var response = await fdc.ExecuteFindAsync(FindReq);

            var responseDataContainsResult = response.Data.Any(r => r.FieldData.Any(v => v.Value.Contains("Buzz")));

            Assert.True(responseDataContainsResult);
        }

        [Fact]
        public async Task FindWithoutQuery_ShouldThrowArgumentException()
        {
            var fdc = GetMockedFDC();

            await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.ExecuteFindAsync(new FindRequest() { Layout = "layout" }));
        }
    }
}