using System;
using System.Net.Http;
using System.Threading.Tasks;
using FMData.Rest.Requests;
using FMData.Rest.Tests.TestModels;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Rest.Tests
{
    /// <summary>
    /// The Create Async Methods All Live in the abstract base class and call SendAsync internally.
    /// These tests validate those base class methods package up the data correctly.
    /// </summary>
    public class CreateAsyncTests
    {
        private static readonly string s_server = "http://localhost";
        private static readonly string s_file = "test-file";
        private static readonly string s_user = "unit";
        private static readonly string s_pass = "test";
        private static readonly string s_layout = "layout";

        private static IFileMakerApiClient GetDataClientWithMockedHandler(MockHttpMessageHandler mockHttp = null)
        {
            if (mockHttp == null)
            {
                // new up a default set of responses (none were provided)
                mockHttp = new MockHttpMessageHandler();

                mockHttp.When(HttpMethod.Post, $"{s_server}/fmi/data/v1/databases/{s_file}/layouts/{s_layout}/records*")
                    .WithPartialContent("fieldData") // make sure that the body content contains the 'data' object expected by fms
                    .Respond("application/json", DataApiResponses.SuccessfulCreate());
            }

            // always add the authentication setup
            mockHttp.When($"{s_server}/fmi/data/v1/databases/{s_file}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), new ConnectionInfo { FmsUri = s_server, Database = s_file, Username = s_user, Password = s_pass });
            return fdc;
        }

        [Fact(DisplayName = "Model With Layout Attribute Should Be Created")]
        public async Task CreateWithTableAttribute_ShouldReturnOK()
        {
            var mockHttp = new MockHttpMessageHandler();

            // since we know 'ModelWithLayout' uses 'Somelayout' as its layout we need to ensure a response for that endpoint
            mockHttp.When(HttpMethod.Post, $"{s_server}/fmi/data/v1/databases/{s_file}/layouts/Somelayout/records*")
                .WithPartialContent("fieldData") // make sure that the body content contains the 'data' object expected by fms
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

            var fdc = GetDataClientWithMockedHandler(mockHttp);

            var newModel = new ModelWithLayout()
            {
                Name = "Fuzzzerd",
                AnotherField = "Different Value"
            };

            var response = await fdc.CreateAsync(newModel);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact(DisplayName = "Layout Parameter Should Allow Creation For Models w/out Attribute")]
        public async Task CreateWithExplicitLayout_ShouldReturnOK()
        {
            var fdc = GetDataClientWithMockedHandler();

            var newModel = new ModelWithoutLayout()
            {
                Name = "Fuzzzerd",
                AnotherField = "Different Value"
            };

            var request = new CreateRequest<ModelWithoutLayout>
            {
                Data = newModel,
                Layout = s_layout
            };
            var response = await fdc.SendAsync(request);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact(DisplayName = "Should Return RecordId Of Created Row")]
        public async Task Create_ShouldReturn_RecordId()
        {
            var mockHttp = new MockHttpMessageHandler();

            // since we know 'ModelWithLayout' uses 'Somelayout' as its layout we need to ensure a response for that endpoint
            mockHttp.When(HttpMethod.Post, $"{s_server}/fmi/data/v1/databases/{s_file}/layouts/Somelayout/records*")
                .WithPartialContent("fieldData") // make sure that the body content contains the 'data' object expected by fms
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

            var fdc = GetDataClientWithMockedHandler(mockHttp);

            var newModel = new ModelWithLayout()
            {
                Name = "Fuzzzerd",
                AnotherField = "Different Value"
            };

            var response = await fdc.CreateAsync(newModel);

            Assert.NotNull(response);
            Assert.Equal(254, response.Response.RecordId);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact(DisplayName = "Model Without Layout and No Layout Parameter Should Throw")]
        public async Task CreateWithoutTableAttribute_ShouldThrow_WithoutExplicitLayout()
        {
            var fdc = GetDataClientWithMockedHandler();

            var newModel = new ModelWithoutLayout()
            {
                Name = "Fuzzzerd",
                AnotherField = "Different Value"
            };

            await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.CreateAsync(newModel));
        }
    }
}
