using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FMData.Rest.Requests;
using FMData.Tests.TestModels;
using RichardSzalay.MockHttp;
using Xunit;
using System.Linq;
using FMData.Rest;

namespace FMData.Tests
{
    public class StrongTypeFindTests
    {
        private static IFileMakerApiClient GetMockedFDC()
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

            var fdc = new DataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);
            return fdc;
        }
        private IFindRequest<User> FindReq => (IFindRequest<User>)new FindRequest<User>()
        {
            Query = new List<User>()
            {
                new User()
                {
                    Id =1
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
        public async Task DictionaryRequest_ForStrongType_ShouldReturn()
        {
            // arrange
            var fdc = GetMockedFDC();

            // act
            var response = await fdc.FindAsync<User>((IFindRequest<Dictionary<string,string>>)new FindRequest<Dictionary<string, string>>()
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
            });

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

            var fdc = new DataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);

            // act
            var response = await fdc.FindAsync<User>(new User()
            {
                Name = "fuzzzerd"
            });

            // assert
            var responseDataContainsResult = response.Any(r => r.Name.Contains("Buzz"));
            Assert.True(responseDataContainsResult);
        }

        [Fact]
        public async Task FindNotFound_Should_ReturnEmpty()
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

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/rest/api/find/{file}/nottherequestbelow")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = new DataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);

            // act
            var response = await fdc.FindAsync<User>(FindReq);

            // assert
            Assert.Empty(response);
        }

        [Fact]
        public async Task FindStrongType_NotFound_Should_ReturnEmpty()
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

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/rest/api/find/{file}/nottherequestbelow")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = new DataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);

            // act
            var toFind = new User() { Id = 35 };
            var response = await fdc.FindAsync<User>(toFind);

            // assert
            Assert.Empty(response);
        }
    }
}