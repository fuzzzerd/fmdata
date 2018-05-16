using FMData.Rest;
using FMData.Rest.Requests;
using FMData.Tests.TestModels;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FMData.Tests
{
    public class FindRequestTests
    {
        private static IFileMakerApiClient GetMockedFDC()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "layout";

            mockHttp.When(HttpMethod.Get, $"{server}/fmi/rest/api/record/{file}/{layout}*")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/rest/api/auth/{file}")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());


            mockHttp.When(HttpMethod.Post, $"{server}/fmi/rest/api/find/{file}/*")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);
            return fdc;
        }

        private IFindRequest<Dictionary<string, string>> FindReq => new FindRequest<Dictionary<string, string>>()
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

        private IFindRequest<User> FindUserReq => (IFindRequest<User>)new FindRequest<User>()
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
        public async Task SendAsync_Find_Should_ReturnData()
        {
            var fdc = GetMockedFDC();

            var response = await fdc.SendAsync(FindReq);

            var responseDataContainsResult = response.Data.Any(r => r.FieldData.Any(v => v.Value.Contains("Buzz")));

            Assert.True(responseDataContainsResult);
        }

        [Fact]
        public async Task SendAsync_EmptyFind_ShouldReturnMany()
        {
            var fdc = GetMockedFDC();

            var response = await fdc.SendAsync(new FindRequest<User> { Layout = "layout" });

            Assert.Equal(2, response.Count());
        }

        [Fact]
        public async Task SendAsync_FindWithoutQuery_ShouldConvertToGetRange_AndReturnMany()
        {
            var fdc = GetMockedFDC();

            var response = await fdc.SendAsync(new FindRequest<Dictionary<string, string>>() { Layout = "layout" });

            Assert.Equal(2, response.Data.Count());
        }

        [Fact]
        public async Task SendAsync_FindWithoutlayout_ShouldThrowArgumentException()
        {
            var fdc = GetMockedFDC();

            await Assert.ThrowsAsync<ArgumentException>(async() => await fdc.SendAsync(new FindRequest<Dictionary<string, string>>() { }));
        }

        [Fact]
        public async Task SendAsync_StrongType_FindShould_ReturnData()
        {
            // arrange
            var fdc = GetMockedFDC();

            // act
            var response = await fdc.SendAsync(FindUserReq);

            // assert
            var responseDataContainsResult = response.Any(r => r.Name.Contains("Buzz"));
            Assert.True(responseDataContainsResult);
        }

        [Fact]
        public async Task Find_WithoutExplicitRequest_ShouldReturnData()
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

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);

            // act
            var response = await fdc.FindAsync(new User()
            {
                Name = "fuzzzerd"
            });

            // assert
            var responseDataContainsResult = response.Any(r => r.Name.Contains("Buzz"));
            Assert.True(responseDataContainsResult);
        }

        [Fact]
        public async Task SendAsync_FindNotFound_Should_ReturnEmpty()
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

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);

            // act
            var response = await fdc.SendAsync(FindUserReq);

            // assert
            Assert.Empty(response);
        }

        [Fact]
        public async Task Find_NotFound_Should_ReturnEmpty()
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

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);

            // act
            var toFind = new User() { Id = 35 };
            var response = await fdc.FindAsync(toFind);

            // assert
            Assert.Empty(response);
        }
    }
}