using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using RichardSzalay.MockHttp;
using Xunit;
using FMData.Rest;
using System.Net;
using System.Net.Http;

// this is apparently necessary to work in appveyor / myget
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace FMData.Tests
{
    public class AuthenticationTests
    {
        [Fact]
        public void NewUp_DataClient_WithTrailingSlash_ShouldBeAuthenticated()
        {
            using (var mockHttp = new MockHttpMessageHandler())
            {
                var server = "http://localhost/";
                var file = "test-file";
                var user = "unit";
                var pass = "test";

                // note the lack of slash here vs other tests to ensure the actual auth endpoint is correctly mocked/hit
                mockHttp.When($"{server}fmi/data/v1/databases/{file}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

                mockHttp.When(HttpMethod.Delete, $"{server}fmi/data/v1/databases/{file}/sessions*")
                .Respond(HttpStatusCode.OK, "application/json", "");

                using (var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass))
                {
                    Assert.True(fdc.IsAuthenticated);
                }

            }
        }

        [Fact]
        public void NewUp_DataClient_ShouldBeAuthenticated()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";

            mockHttp.When($"{server}/fmi/data/v1/databases/{file}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Delete, $"{server}/fmi/data/v1/databases/{file}/sessions*")
                .Respond(HttpStatusCode.OK, "application/json", "");

            using (var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass))
            {
                Assert.True(fdc.IsAuthenticated);
            }
        }

        [Fact]
        public void DataCliet_ShouldHandle_401_From_Sessions()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                .Respond(HttpStatusCode.Unauthorized, "application/json", DataApiResponses.Authentication401());

            mockHttp.When(HttpMethod.Delete, $"{server}/fmi/data/v1/databases/{file}/sessions*")
                .Respond(HttpStatusCode.OK, "application/json", "");

            using (var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass))
            {
                Assert.False(fdc.IsAuthenticated);
            }
        }

        [Fact]
        public async Task RefreshToken_ShouldGet_NewToken()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";

            mockHttp.When($"{server}/fmi/data/v1/databases/{file}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication("someOtherToken"));

            mockHttp.When(HttpMethod.Delete, $"{server}/fmi/data/v1/databases/{file}/sessions*")
                .Respond(HttpStatusCode.OK, "application/json", "");

            using (var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass))
            {
                var response = await fdc.RefreshTokenAsync("integration", "test");
                Assert.Equal("someOtherToken", response.Response.Token);
            }
        }

        [Theory]
        [InlineData("", "test")]
        [InlineData("integration", "")]
        public async Task RefreshToken_Requires_AllParameters(string user, string pass)
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";

            mockHttp.When($"{server}/fmi/data/v1/databases/{file}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication("someOtherToken"));

            mockHttp.When(HttpMethod.Delete, $"{server}/fmi/data/v1/databases/{file}/sessions*")
    .Respond(HttpStatusCode.OK, "application/json", "");

            // pass in actual values here since we DON'T want this to blow up on constructor 
            using (var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, "user", "pass"))
            {
                await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.RefreshTokenAsync(user, pass));
            }
        }
    }
}
