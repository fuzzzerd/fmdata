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

                mockHttp.When(HttpMethod.Post, $"{s_server}/fmi/data/v1/databases/{s_file}/layouts/Somelayout/records*")
                    .WithPartialContent("fieldData") // make sure that the body content contains the 'data' object expected by fms
                    .Respond("application/json", DataApiResponses.SuccessfulCreate());
            }

            // always add the authentication setup
            mockHttp.When($"{s_server}/fmi/data/v1/databases/{s_file}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), new ConnectionInfo { FmsUri = s_server, Database = s_file, Username = s_user, Password = s_pass });
            return fdc;
        }

        private readonly ICreateRequest<Dictionary<string, string>> _reqWithoutLayout = new CreateRequest<Dictionary<string, string>>()
        {
            Data = new Dictionary<string, string>()
                {
                    { "Name", "Fuzzerd" },
                    { "AnotherField", "Another Value" }
                }
        };

        private readonly ICreateRequest<Dictionary<string, string>> _reqWithLayout = new CreateRequest<Dictionary<string, string>>()
        {
            Layout = s_layout,
            Data = new Dictionary<string, string>()
                {
                    { "Name", "Fuzzerd" },
                    { "AnotherField", "Another Value" }
                }
        };

        [Fact(DisplayName = "Without Layout Create Record Should Throw")]
        public async Task Create_WithoutLayout_ThrowsArgumentException_SendAsync()
        {
            var fdc = GetDataClientWithMockedHandler();

            await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.SendAsync(_reqWithoutLayout));
        }

        [Fact(DisplayName = "With Layout Create Should Succeed")]
        public async Task CreateFromDictionaryStringString_ShouldReturnOK_FromSendAsync()
        {
            var fdc = GetDataClientWithMockedHandler();

            var response = await fdc.SendAsync(_reqWithLayout);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact(DisplayName = "Script Should Be In JSON")]
        public async Task CreateWithScript_RequiresScriptSet()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{s_server}/fmi/data/v1/databases/{s_file}/layouts/{s_layout}/records*")
                .WithPartialContent("fieldData")
                .WithPartialContent("script") // ensure the body contains the script parameter we're sending
                .WithPartialContent("run_this_script")
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

            var fdc = GetDataClientWithMockedHandler(mockHttp);

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

            mockHttp.When(HttpMethod.Post, $"{s_server}/fmi/data/v1/databases/{s_file}/layouts/{s_layout}/records*")
                .WithPartialContent("fieldData")
                .WithPartialContent("script.prerequest") // ensure the body contains the script parameter we're sending
                .WithPartialContent("run_this_script")
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

            var fdc = GetDataClientWithMockedHandler(mockHttp);

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

            mockHttp.When(HttpMethod.Post, $"{s_server}/fmi/data/v1/databases/{s_file}/layouts/{s_layout}/records*")
                .WithPartialContent("fieldData")
                .WithPartialContent("script.presort") // ensure the body contains the script parameter we're sending
                .WithPartialContent("run_this_script")
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

            var fdc = GetDataClientWithMockedHandler(mockHttp);

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

            mockHttp.When(HttpMethod.Post, $"{s_server}/fmi/data/v1/databases/{s_file}/layouts/{s_layout}/records*")
                .WithPartialContent("fieldData")
                .WithPartialContent("script.presort") // ensure the body contains the script parameter we're sending
                .WithPartialContent("run_this_script_psort")
                .WithPartialContent("script.prerequest") // ensure the body contains the script parameter we're sending
                .WithPartialContent("run_this_script_preq")
                .WithPartialContent("script") // ensure the body contains the script parameter we're sending
                .WithPartialContent("run_this_script_reg")
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

            var fdc = GetDataClientWithMockedHandler(mockHttp);

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
        [Fact(DisplayName = "Set Global Should Generate Correct Json")]
        public async Task SetGlobal_Should_Generate_Valid_Json()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(new HttpMethod("PATCH"), $"{s_server}/fmi/data/v1/databases/{s_file}/globals")
                .WithPartialContent("{\"globalFields\":{\"Table::Field\":\"Value\\nValue\"}}") // ensure newline is properly escaped
                .Respond("application/json", DataApiResponses.SetGlobalSuccess());

            var fdc = GetDataClientWithMockedHandler(mockHttp);

            var globalResponse = await fdc.SetGlobalFieldAsync("Table", "Field", "Value\nValue");

            Assert.NotNull(globalResponse);
            Assert.Contains(globalResponse.Messages, r => r.Message == "OK");
        }

        /// <summary>
        /// Global Field / PATCH
        /// </summary>
        /// <returns></returns>
        [Fact(DisplayName = "Set Global Should Have FMS Compatible GlobalField attributes In JSON")]
        public async Task SetGlobal_Should_Have_Valid_Global_In_Json()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(new HttpMethod("PATCH"), $"{s_server}/fmi/data/v1/databases/{s_file}/globals")
                .WithPartialContent("{\"globalFields\":{\"Table::Field\":\"Value\"}}")
                .Respond("application/json", DataApiResponses.SetGlobalSuccess());

            var fdc = GetDataClientWithMockedHandler(mockHttp);

            var globalResponse = await fdc.SetGlobalFieldAsync("Table", "Field", "Value");

            Assert.NotNull(globalResponse);
            Assert.Contains(globalResponse.Messages, r => r.Message == "OK");
        }

        /// <summary>
        /// Global Field / PATCH
        /// </summary>
        /// <returns></returns>
        [Fact(DisplayName = "Set Global Should Have Global In JSON")]
        public async Task SetGlobal_Should_Have_Global_In_Json()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(new HttpMethod("PATCH"), $"{s_server}/fmi/data/v1/databases/{s_file}/globals")
                .WithPartialContent("{\"globalFields\":{\"Table::Field\":\"Value\"}}")
                .Respond("application/json", DataApiResponses.SetGlobalSuccess());

            var fdc = GetDataClientWithMockedHandler(mockHttp);

            var globalResponse = await fdc.SetGlobalFieldAsync("Table", "Field", "Value");

            Assert.NotNull(globalResponse);
            Assert.Contains(globalResponse.Messages, r => r.Message == "OK");
        }

        /// <summary>
        /// Global Field / PATCH
        /// </summary>
        /// <returns></returns>
        [Fact(DisplayName = "Set Global Should Throw When Table Is Not Provided")]
        public async Task SetGlobal_Should_Throw_When_Table_Not_Specified()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(new HttpMethod("PATCH"), $"{s_server}/fmi/data/v1/databases/{s_file}/globals")
                .WithPartialContent("globalFields")
                .Respond("application/json", DataApiResponses.SetGlobalSuccess());

            var fdc = GetDataClientWithMockedHandler(mockHttp);

            await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.SetGlobalFieldAsync(null, "Field", "Value"));
        }

        /// <summary>
        /// Global Field / PATCH
        /// </summary>
        /// <returns></returns>
        [Fact(DisplayName = "Set Global Should Throw When Field Is Not Provided")]
        public async Task SetGlobal_Should_Throw_When_Field_Not_Specified()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(new HttpMethod("PATCH"), $"{s_server}/fmi/data/v1/databases/{s_file}/globals")
                .WithPartialContent("globalFields")
                .Respond("application/json", DataApiResponses.SetGlobalSuccess());

            var fdc = GetDataClientWithMockedHandler(mockHttp);

            await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.SetGlobalFieldAsync("Table", null, "Value"));
        }

        /// <summary>
        /// Global Field / PATCH
        /// </summary>
        /// <returns></returns>
        [Fact(DisplayName = "Set Global Should Throw When Value Is Not Provided")]
        public async Task SetGlobal_Should_Throw_When_Value_Not_Specified()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(new HttpMethod("PATCH"), $"{s_server}/fmi/data/v1/databases/{s_file}/globals")
                .WithPartialContent("globalFields")
                .Respond("application/json", DataApiResponses.SetGlobalSuccess());

            var fdc = GetDataClientWithMockedHandler(mockHttp);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await fdc.SetGlobalFieldAsync("Table", "Field", null));
        }
    }
}
