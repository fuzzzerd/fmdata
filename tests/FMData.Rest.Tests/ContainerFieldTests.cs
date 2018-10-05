using FMData.Rest.Tests.TestModels;
using RichardSzalay.MockHttp;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FMData.Rest.Tests
{
    public class ContainerFieldTests
    {
        [Fact(DisplayName ="Container Field Update Should Post Upload")]
        public async Task EditContainer_Should_PostUpload()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "layout";

            mockHttp.When($"{server}/fmi/data/v1/databases/{file}/sessions")
               .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records/*")
                .WithPartialContent("upload")
                .Respond("application/json", DataApiResponses.SuccessfulEdit());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);

            var b64String = System.IO.File.ReadAllText("ResponseData\\b64-string.dat");
            var bytes = Convert.FromBase64String(b64String);

            var response = await fdc.UpdateContainerAsync(layout, 12, "field", "test.jpg", bytes);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Theory]
        [InlineData((string)null)]
        [InlineData("//somefile")]
        [InlineData("http:/localhost/somefile")]
        [InlineData("s:localhost/")]
        [InlineData("somefolder/somefile.ext")]
        public async Task ProcessContainerData_Should_Skip_InvalidUris(string uri)
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);

            var model = new ContainerFieldTestModel
            {
                SomeContainerField = uri
            };

            // act
            await fdc.ProcessContainer(model);

            // assert
            Assert.Null(model.SomeContainerFieldData);
        }
    }
}