using FMData.Rest.Tests.TestModels;
using RichardSzalay.MockHttp;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FMData.Rest.Tests
{
    /// <summary>
    /// The Create Async Methods All Live in the abstract base class and call SendAsync internally.
    /// These tests validate those base class methods package up the data correctly.
    /// </summary>
    public class CreateAsyncTests
    {
        private static readonly string server = "http://localhost";
        private static readonly string file = "test-file";
        private static readonly string user = "unit";
        private static readonly string pass = "test";
        private static readonly string layout = "layout";

        private static IFileMakerApiClient GetDataClientWithMockedHandler(MockHttpMessageHandler mockHttp = null)
        {
            if(mockHttp == null)
            {
                // new up a default set of responses (none were provided)
                mockHttp = new MockHttpMessageHandler();

                mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records*")
                    .WithPartialContent("fieldData") // make sure that the body content contains the 'data' object expected by fms
                    .Respond("application/json", DataApiResponses.SuccessfulCreate());
            }

            // always add the authentication setup
            mockHttp.When($"{server}/fmi/data/v1/databases/{file}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);
            return fdc;
        }

        [Fact(DisplayName = "Model With Layout Attribute Should Be Created")]
        public async Task CreateWithTableAttribute_ShouldReturnOK()
        {
            var mockHttp = new MockHttpMessageHandler();

            // since we know 'ModelWithLayout' uses 'Somelayout' as its layout we need to ensure a response for that endpoint
            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/Somelayout/records*")
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
            Assert.Contains(response.Messages, r=> r.Message == "OK");
        }

        [Fact(DisplayName ="Layout Parameter Should Allow Creation For Models w/out Attribute")]
        public async Task CreateWithExplicitLayout_ShouldReturnOK()
        {
            var fdc = GetDataClientWithMockedHandler();

            var newModel = new ModelWithoutLayout()
            {
                Name = "Fuzzzerd",
                AnotherField = "Different Value"
            };

            var response = await fdc.CreateAsync(layout, newModel);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        /// <summary>
        /// this would be 'Somelayout' but we don't provide a response on that uri for 
        /// this test, so a valid response means the layout paramater was used.
        /// </summary>
        [Fact(DisplayName = "Layout Parameter Should Override Model Attribute")]
        public async Task Layout_Parameter_Should_Override_Model_Attribute()
        {
            var fdc = GetDataClientWithMockedHandler();

            var newModel = new ModelWithLayout()
            {
                Name = "Fuzzzerd",
                AnotherField = "Different Value"
            };

            var response = await fdc.CreateAsync(layout, newModel);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact(DisplayName ="Should Return RecordId Of Created Row")]
        public async Task Create_ShouldReturn_RecordId()
        {
            var mockHttp = new MockHttpMessageHandler();

            // since we know 'ModelWithLayout' uses 'Somelayout' as its layout we need to ensure a response for that endpoint
            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/Somelayout/records*")
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

        [Fact(DisplayName ="Model Without Layout and No Layout Parameter Should Throw")]
        public async Task CreateWithoutTableAttirbute_ShouldThrow_WithoutExplicitLayout()
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