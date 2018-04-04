using System;
using System.Threading.Tasks;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMREST.Tests
{
    public class AuthenticationTests
    {
        [Fact]
        public void TestAuthShouldSucceed()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "layout";

            mockHttp.When($"{server}/fmi/rest/api/auth/{file}")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var fdc = new FMDataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);

            Assert.True(fdc.IsAuthenticated);
        }
    }
}
