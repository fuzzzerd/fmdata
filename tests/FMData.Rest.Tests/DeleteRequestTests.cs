using System.Threading.Tasks;
using FMData.Rest;
using FMData.Rest.Requests;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Tests
{
    public class DeleteRequestTests
    {
        private IFileMakerApiClient GetMockedClient()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "layout";

            mockHttp.When($"{server}/fmi/rest/api/auth/{file}")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When($"{server}/fmi/rest/api/record/{file}/{layout}/*")
                .Respond("application/json", DataApiResponses.SuccessfulDelete());

            var mockedClient = mockHttp.ToHttpClient();

            var fdc = new FileMakerRestClient(mockedClient, server, file, user, pass, layout);
            return fdc;
        }

        [Fact]
        public async Task DeleteShould_ReturnOK()
        {
            var fdc = GetMockedClient();

            var req = new DeleteRequest()
            {
                Layout = "layout",
                RecordId = "1234"
            };
            var response = await fdc.SendAsync(req);

            Assert.NotNull(response);
            Assert.Equal("OK", response.Result);
        }

        [Fact]
        public async Task DeleteByIdandLayout_Should_ReturnOK()
        {
            var fdc = GetMockedClient();

            var response = await fdc.DeleteAsync(2, "layout");

            Assert.NotNull(response);
            Assert.Equal("OK", response.Result);
        }

        [Fact]
        public async Task DeleteByModel_Should_ReturnOK()
        {
            // arrange 
            // NOT DRY, but used to specify special request endpoints
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "Users";

            mockHttp.When($"{server}/fmi/rest/api/auth/{file}")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When($"{server}/fmi/rest/api/record/{file}/{layout}/*")
                .Respond("application/json", DataApiResponses.SuccessfulDelete());

            var mockedClient = mockHttp.ToHttpClient();

            var fdc = new FileMakerRestClient(mockedClient, server, file, user, pass, layout);

            var toDelete = new TestModels.User();
            // act
            var response = await fdc.DeleteAsync(2, toDelete);
            // assert
            Assert.NotNull(response);
            Assert.Equal("OK", response.Result);
        }

        [Fact]
        public async Task DeleteByWrongLayout_Should_ReturnFourOhFour()
        {
            // arrange 
            var fdc = GetMockedClient();
            var toDelete = new TestModels.User();

            // act
            var response = await fdc.DeleteAsync(2, toDelete);

            // assert
            Assert.NotNull(response);
            Assert.Equal("404", response.ErrorCode);
            Assert.Equal("Error", response.Result);
        }
    }
}