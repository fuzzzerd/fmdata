using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using RichardSzalay.MockHttp;
using Xunit;
using FMData.Rest;

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
                var layout = "layout";

                // note the lack of slash here vs other tests to ensure the actual auth endpoint is correctly mocked/hit
                mockHttp.When($"{server}fmi/rest/api/auth/{file}")
                        .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

                using (var fdc = new DataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout))
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
            var layout = "layout";

            mockHttp.When($"{server}/fmi/rest/api/auth/{file}")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            using (var fdc = new DataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout))
            {
                Assert.True(fdc.IsAuthenticated);
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
            var layout = "layout";

            mockHttp.When($"{server}/fmi/rest/api/auth/{file}")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication("someOtherToken"));

            using (var fdc = new DataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout))
            {
                var response = await fdc.RefreshTokenAsync("integration", "test", "someLayout");
                Assert.Equal("someOtherToken", response.Token);
            }
        }

        [Theory]
        [InlineData("", "test", "layout")]
        [InlineData("integration", "", "layout")]
        [InlineData("integration", "test", "")]
        public async Task RefreshToken_Requires_AllParameters(string user, string pass, string layout)
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";

            mockHttp.When($"{server}/fmi/rest/api/auth/{file}")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication("someOtherToken"));

            // pass in actual values here since we DON'T want this to blow up on constructor 
            using (var fdc = new DataClient(mockHttp.ToHttpClient(), server, file, "user", "pass", "layout"))
            {
                await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.RefreshTokenAsync(user, pass, layout));
            }
        }
    }
}