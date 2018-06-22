using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FMData;
using FMData.Rest;
using FMData.Rest.Requests;
using FMData.Tests.TestModels;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Tests
{
    public class CreateRequestTests
    {
        private static string server = "http://localhost";
        private static string file = "test-file";
        private static string user = "unit";
        private static string pass = "test";
        private static string layout = "layout";

        [Table("Somelayout")]
        public class TableModelTest
        {
            public string Name { get; set; }
            public string AnotherField { get; set; }
        }

        public class ModelTest
        {
            public string Name { get; set; }
            public string AnotherField { get; set; }
        }

        private static IFileMakerApiClient GetDataClientWithMockedHandler(MockHttpMessageHandler mockHttp = null)
        {
            if(mockHttp == null) {
                // new up a default set of responses (none were provided)
                mockHttp = new MockHttpMessageHandler();

                mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records*")
                .WithPartialContent("fieldData") // make sure that the body content contains the 'data' object expected by fms
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

                mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/Somelayout/records*")
                    .WithPartialContent("fieldData") // make sure that the body content contains the 'data' object expected by fms
                    .Respond("application/json", DataApiResponses.SuccessfulCreate());
            }
            
            // always add the authentication setup
            mockHttp.When($"{server}/fmi/data/v1/databases/{file}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);
            return fdc;
        }

        [Fact]
        public async Task CreateWithTableAttribute_ShouldReturnOK()
        {
            var fdc = GetDataClientWithMockedHandler();

            var newModel = new TableModelTest()
            {
                Name = "Fuzzzerd",
                AnotherField = "Different Value"
            };

            var response = await fdc.CreateAsync(newModel);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r=> r.Message == "OK");
        }

        [Fact]
        public async Task CreateWithExplicitLayout_ShouldReturnOK()
        {
            var fdc = GetDataClientWithMockedHandler();

            var newModel = new ModelTest()
            {
                Name = "Fuzzzerd",
                AnotherField = "Different Value"
            };

            var response = await fdc.CreateAsync("layout", newModel);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact]
        public async Task CreateShould_ReturnNewRecordId()
        {
            var fdc = GetDataClientWithMockedHandler();

            var newModel = new ModelTest()
            {
                Name = "Fuzzzerd",
                AnotherField = "Different Value"
            };

            var response = await fdc.CreateAsync("layout", newModel);

            Assert.NotNull(response);
            Assert.Equal(254, response.Response.RecordId);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact]
        public async Task Create_ShouldReturn_RecordId()
        {
            var fdc = GetDataClientWithMockedHandler();

            var newModel = new TableModelTest()
            {
                Name = "Fuzzzerd",
                AnotherField = "Different Value"
            };

            var response = await fdc.CreateAsync(newModel);

            Assert.NotNull(response);
            Assert.Equal(254, response.Response.RecordId);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact]
        public async Task CreateWithoutTableAttirbute_ShouldThrow_WithoutExplicitLayout()
        {
            var fdc = GetDataClientWithMockedHandler();

            var newModel = new ModelTest()
            {
                Name = "Fuzzzerd",
                AnotherField = "Different Value"
            };

            await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.CreateAsync(newModel));
        }

        [Fact]
        public async Task Create_WithoutLayout_ThrowsArgumentException_SendAsync()
        {
            IFileMakerApiClient fdc = GetDataClientWithMockedHandler();

            var req = new CreateRequest<Dictionary<string, string>>()
            {
                Data = new Dictionary<string, string>()
                {
                    { "Name", "Fuzzerd" },
                    { "AnotherField", "Another Valuee" }
                }
            };

            await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.SendAsync(req));
        }


        [Fact]
        public async Task CreateFromDictionaryStringString_ShouldReturnOK_FromSendAsync()
        {
            IFileMakerApiClient fdc = GetDataClientWithMockedHandler();

            var req = new CreateRequest<Dictionary<string, string>>()
            {
                Layout = "layout",
                Data = new Dictionary<string, string>()
                {
                    { "Name", "Fuzzerd" },
                    { "AnotherField", "Another Valuee" }
                }
            };

            // requires cast to call correct method -- maybe needs renamed since overloading isn't working out so well
            var response = await fdc.SendAsync(req);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact]
        public async Task CreateWithScript_RequiresScriptSet()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records*")
                .WithPartialContent("fieldData")
                .WithPartialContent("script") // ensure the body contains the script parameter we're sending
                .WithPartialContent("run_this_script")
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

            IFileMakerApiClient fdc = GetDataClientWithMockedHandler(mockHttp);

            var req = new CreateRequest<User>()
            {
                Layout = "layout",
                Data = new User { Name = "test name" },
                Script = "run_this_script"
            };

            var response = await fdc.SendAsync(req);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact]
        public async Task CreateWithPreRequestScript_RequiresScriptSet()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records*")
                .WithPartialContent("fieldData")
                .WithPartialContent("script.prerequest") // ensure the body contains the script parameter we're sending
                .WithPartialContent("run_this_script")
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

            IFileMakerApiClient fdc = GetDataClientWithMockedHandler(mockHttp);

            var req = new CreateRequest<User>()
            {
                Layout = "layout",
                Data = new User { Name = "test name" },
                PreRequestScript = "run_this_script"
            };

            var response = await fdc.SendAsync(req);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact]
        public async Task CreateWithPreSortScript_RequiresScriptSet()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records*")
                .WithPartialContent("fieldData")
                .WithPartialContent("script.presort") // ensure the body contains the script parameter we're sending
                .WithPartialContent("run_this_script")
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

            IFileMakerApiClient fdc = GetDataClientWithMockedHandler(mockHttp);

            var req = new CreateRequest<User>()
            {
                Layout = "layout",
                Data = new User { Name = "test name" },
                PreSortScript = "run_this_script"
            };

            var response = await fdc.SendAsync(req);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact]
        public async Task CreateWithGlobal_Should_GetTwoOKs()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(new HttpMethod("PATCH"), $"{server}/fmi/data/v1/databases/{file}/globals")
                .WithPartialContent("globalFields")
                .Respond("application/json", DataApiResponses.SetGlobalSuccess());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records*")
                .WithPartialContent("fieldData")
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

            IFileMakerApiClient fdc = GetDataClientWithMockedHandler(mockHttp);

            var req = new CreateRequest<User>()
            {
                Layout = "layout",
                Data = new User { Name = "test name" },
                Script = "run_this_script"
            };

            var globalResponse = await fdc.SetGlobalFieldAsync("Table", "Field", "Value");
            var response = await fdc.SendAsync(req);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");

            Assert.NotNull(globalResponse);
            Assert.Contains(globalResponse.Messages, r => r.Message == "OK");
        }
    }
}