using FMData.Rest.Requests;
using FMData.Rest.Tests.TestModels;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FMData.Rest.Tests
{
    public class FindAsyncTests
    {
        private static readonly string server = "http://localhost";
        private static readonly string file = "test-file";
        private static readonly string user = "unit";
        private static readonly string pass = "test";

        private static IFileMakerApiClient GetMockedFDC()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
               .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Get, $"{server}/fmi/data/v1/databases/{file}/layout*")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/layout/_find")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/Users/_find")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);
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

        private IFindRequest<User> FindUserReqWithLayoutOverride => new FindRequest<User>()
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
        public async Task FindAsync_WithSkipTake_ShouldHave_LimitOffset()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var layout = "Users";

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/_find")
                .WithPartialContent("limit") // ensure the request contains the expected content
                .WithPartialContent("offset") // ensure the request contains the expected content
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);

            // act
            var response = await fdc.FindAsync(new User()
            {
                Name = "fuzzzerd"
            }, 5, 5);

            // assert
            Assert.NotEmpty(response);
        }

        [Fact]
        public async Task Find_WithScript_ShouldHaveScript()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var layout = "Users";

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/_find")
                .WithPartialContent("fuzzzerd") // ensure the request contains the expected content
                .WithPartialContent("script").WithPartialContent("nos_ran")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);

            // act
            var response = await fdc.FindAsync(new User()
            {
                Name = "fuzzzerd"
            }, "nos_ran", null, null);

            // assert
            var responseDataContainsResult = response.Any(r => r.Created == DateTime.Parse("03/29/2018 15:22:09"));
            Assert.True(responseDataContainsResult);
        }

        [Fact]
        public async Task Find_NotFound_Should_ReturnEmpty()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var layout = FileMakerRestClient.GetTableName(new User());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/_find")
                .Respond(HttpStatusCode.InternalServerError, "application/json", DataApiResponses.FindNotFound());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);

            // act
            var toFind = new User() { Id = 35 };
            var response = await fdc.FindAsync(toFind);

            // assert
            Assert.Empty(response);
        }

        [Fact]
        public async Task FindAsync_WithPortals_ShouldHaveData()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/_find")
                .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFindWithPortal());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);

            var toFind = new PortalModel() { ES_ONE = "" };

            // act
            var response = await fdc.FindAsync(toFind);

            // assert
            Assert.NotEmpty(response);
            Assert.NotEmpty(response.SelectMany(p => p.Actions));
            // hard coded from sample data, if changed update here
            Assert.Equal(16, response.First().Actions.First().ID);
        }
    }
}