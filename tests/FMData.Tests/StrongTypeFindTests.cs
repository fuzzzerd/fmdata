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
        private FindRequest<User> FindReq => new FindRequest<User>()
        {
            Query = new List<User>()
            {
                new User()
                {
                    Name ="fuzzzerd"
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

        [Fact]
        public async Task StrongType_Find_WithoutExplicitRequest_ShouldReturnData()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "layout";

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/rest/api/auth/{file}")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/rest/api/find/{file}/*")
                .WithPartialContent("fuzzzerd") // ensure the request contains the expected content
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FMDataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);

            // act
            var response = await fdc.FindAsync<User>(new User()
            {
                Name = "fuzzzerd"
            });

            // assert
            var responseDataContainsResult = response.Any(r => r.Name.Contains("Buzz"));
            Assert.True(responseDataContainsResult);
        }
    }
}