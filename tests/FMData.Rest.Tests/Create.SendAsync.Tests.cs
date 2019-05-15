using FMData.Rest.Requests;
using FMData.Rest.Tests.TestModels;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FMData.Rest.Tests
{
    /// <summary>
    /// Main Test Cases for Creating Records; and ensureing the proper JSON is generated for sending to FMS.
    /// </summary>
    public class CreateSendAsyncTests
    {
        private static readonly string server = "http://localhost";
        private static readonly string file = "test-file";
        private static readonly string user = "unit";
        private static readonly string pass = "test";
        private static readonly string layout = "layout";

        private static IFileMakerApiClient GetDataClientWithMockedHandler(MockHttpMessageHandler mockHttp = null)
        {
            if (mockHttp == null)
            {
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

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), new ConnectionInfo { FmsUri = server, Database = file, Username = user, Password = pass });
            return fdc;
        }

        private readonly ICreateRequest<Dictionary<string, string>> reqWithoutLayout = new CreateRequest<Dictionary<string, string>>()
        {
            Data = new Dictionary<string, string>()
                {
                    { "Name", "Fuzzerd" },
                    { "AnotherField", "Another Valuee" }
                }
        };

        private readonly ICreateRequest<Dictionary<string, string>> reqWithLayout = new CreateRequest<Dictionary<string, string>>()
        {
            Layout = layout,
            Data = new Dictionary<string, string>()
                {
                    { "Name", "Fuzzerd" },
                    { "AnotherField", "Another Valuee" }
                }
        };

        [Fact(DisplayName = "Without Layout Create Record Should Throw")]
        public async Task Create_WithoutLayout_ThrowsArgumentException_SendAsync()
        {
            IFileMakerApiClient fdc = GetDataClientWithMockedHandler();

            await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.SendAsync(reqWithoutLayout));
        }

        [Fact(DisplayName ="With Layout Create Should Succeed")]
        public async Task CreateFromDictionaryStringString_ShouldReturnOK_FromSendAsync()
        {
            IFileMakerApiClient fdc = GetDataClientWithMockedHandler();

            var response = await fdc.SendAsync(reqWithLayout);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact(DisplayName ="Script Should Be In JSON")]
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

        [Fact(DisplayName = "Pre-Request Script Should Be In JSON")]
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

        [Fact(DisplayName = "Pre-Sort Script Should Be In JSON")]
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

        [Fact(DisplayName = "All Scripts Should Contain All Scripts in JSON")]
        public async Task CreateRequest_WithAllScripts_ShouldContainAll()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records*")
                .WithPartialContent("fieldData")
                .WithPartialContent("script.presort") // ensure the body contains the script parameter we're sending
                .WithPartialContent("run_this_script_psort")
                .WithPartialContent("script.prerequest") // ensure the body contains the script parameter we're sending
                .WithPartialContent("run_this_script_preq")
                .WithPartialContent("script") // ensure the body contains the script parameter we're sending
                .WithPartialContent("run_this_script_reg")
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

            IFileMakerApiClient fdc = GetDataClientWithMockedHandler(mockHttp);

            var req = new CreateRequest<User>()
            {
                Layout = "layout",
                Data = new User { Name = "test name" },
                Script = "run_this_script_reg",
                PreRequestScript = "run_this_script_preq",
                PreSortScript = "run_this_script_psort"
            };

            var response = await fdc.SendAsync(req);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        /// <summary>
        /// Global Field / PATCH
        /// </summary>
        /// <returns></returns>
        [Fact(DisplayName = "Set Global Should Have Global In JSON")]
        public async Task CreateWithGlobal_Should_GetTwoOKs()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(new HttpMethod("PATCH"), $"{server}/fmi/data/v1/databases/{file}/globals")
                .WithPartialContent("globalFields")
                .Respond("application/json", DataApiResponses.SetGlobalSuccess());

            IFileMakerApiClient fdc = GetDataClientWithMockedHandler(mockHttp);

            var req = new CreateRequest<User>()
            {
                Layout = "layout",
                Data = new User { Name = "test name" },
                Script = "run_this_script"
            };

            var globalResponse = await fdc.SetGlobalFieldAsync("Table", "Field", "Value");

            Assert.NotNull(globalResponse);
            Assert.Contains(globalResponse.Messages, r => r.Message == "OK");
        }
    }
}