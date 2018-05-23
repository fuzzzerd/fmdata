using System.Net.Http;
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

            mockHttp.When($"{server}/fmi/data/v1/databases/{file}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Delete, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records/*")
                .Respond("application/json", DataApiResponses.SuccessfulDelete());

            var mockedClient = mockHttp.ToHttpClient();

            var fdc = new FileMakerRestClient(mockedClient, server, file, user, pass);
            return fdc;
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

            mockHttp.When($"{server}/fmi/data/v1/databases/{file}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Delete, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records/*")
                .Respond("application/json", DataApiResponses.SuccessfulDelete());

            var mockedClient = mockHttp.ToHttpClient();

            var fdc = new FileMakerRestClient(mockedClient, server, file, user, pass);

            // act
            var response = await fdc.DeleteAsync<TestModels.User>(2);

            // assert
            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact]
        public async Task DeleteShould_ReturnOK()
        {
            var fdc = GetMockedClient();

            var req = new DeleteRequest()
            {
                Layout = "layout",
                RecordId = 1234
            };
            var response = await fdc.SendAsync(req);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact]
        public async Task DeleteByIdandLayout_Should_ReturnOK()
        {
            var fdc = GetMockedClient();

            var response = await fdc.DeleteAsync(2, "layout");

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact]
        public async Task DeleteByWrongLayout_Should_ReturnFourOhFour()
        {
            // arrange 
            var fdc = GetMockedClient();

            // act
            var response = await fdc.DeleteAsync<TestModels.User>(2);

            // assert
            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Code == "404");
            Assert.Contains(response.Messages, r => r.Message == "Error");
        }
    }
}