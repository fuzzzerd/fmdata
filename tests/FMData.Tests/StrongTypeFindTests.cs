using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FMData.Requests;
using FMData.Tests.TestModels;
using RichardSzalay.MockHttp;
using Xunit;
using System.Linq;

namespace FMData.Tests
{
    public class StrongTypeFindTests
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
        public async Task StrongType_FindShould_ReturnData()
        {
            // arrange
            var fdc = GetMockedFDC();

            // act
            var response = await fdc.FindAsync<User>(FindReq);

            // assert
            var responseDataContainsResult = response.Any(r => r.Name.Contains("Buzz"));
            Assert.True(responseDataContainsResult);
        }
    }
}