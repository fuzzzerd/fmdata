using FMData.Rest.Requests;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FMData.Rest.Tests
{
    public class DeleteRequestTests
    {
        private static IFileMakerApiClient GetMockedClient()
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

            var fdc = new FileMakerRestClient(mockedClient, new ConnectionInfo { FmsUri = server, Database = file, Username = user, Password = pass });
            return fdc;
        }

        [Fact(DisplayName = "Generic Model And FMRecId Should Delete")]
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

            var fdc = new FileMakerRestClient(mockedClient, new ConnectionInfo { FmsUri = server, Database = file, Username = user, Password = pass });

            // act
            var response = await fdc.DeleteAsync<TestModels.User>(2);

            // assert
            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact(DisplayName = "Layout and ID Should Delete OK")]
        public async Task DeleteByIdAndLayout_Should_ReturnOK()
        {
            var fdc = GetMockedClient();

            var response = await fdc.DeleteAsync(2, "layout");

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact(DisplayName = "Invalid Layout Should Return 404")]
        public async Task DeleteByWrongLayout_Should_ReturnFourOhFour()
        {
            // arrange 
            var fdc = GetMockedClient();

            // act
            var response = await fdc.DeleteAsync(2, "not-valid-layout");

            // assert
            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Code == "404");
            Assert.Contains(response.Messages, r => r.Message == "Error");
        }

        //==\\
        // Send Async
        //==//

        [Fact(DisplayName = "By Delete Request Should Return OK")]
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
        public async Task Delete_Should_Throw_FMDataException_For_InternalServerError()
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
                .Respond(HttpStatusCode.InternalServerError, "application/json", DataApiResponses.LayoutNotFound());

            var mockedClient = mockHttp.ToHttpClient();

            var fdc = new FileMakerRestClient(mockedClient, new ConnectionInfo { FmsUri = server, Database = file, Username = user, Password = pass });

            // act
            // assert
            await Assert.ThrowsAsync<FMDataException>(async () => await fdc.DeleteAsync<TestModels.User>(2));
        }
    }
}
