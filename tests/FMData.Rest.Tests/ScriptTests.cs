using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FMData.Rest.Requests;
using FMData.Rest.Tests.TestModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Rest.Tests
{
    public class ScriptTests
    {
        private static readonly string Server = "http://localhost";
        private static readonly string File = "test-file";
        private static readonly string User = "unit";
        private static readonly string Pass = "test";
        private static readonly string Layout = "layout";

        private static FileMakerRestClient GetClient(MockHttpMessageHandler mockHttp)
        {
            return new FileMakerRestClient(mockHttp.ToHttpClient(), new ConnectionInfo { FmsUri = Server, Database = File, Username = User, Password = Pass });
        }

        private static MockHttpMessageHandler CreateMockWithAuth()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());
            return mockHttp;
        }

        [Fact(DisplayName = "Get Scripts Should Return Script List")]
        public async Task GetScripts_Should_Return_Script_List()
        {
            var mockHttp = CreateMockWithAuth();

            var layoutData = System.IO.File.ReadAllText(Path.Combine("ResponseData", "ScriptList.json"));
            mockHttp.When($"{Server}/fmi/data/v1/databases/{File}/scripts")
                    .Respond("application/json", layoutData);

            var fdc = GetClient(mockHttp);

            var response = await fdc.GetScriptsAsync();

            Assert.NotNull(response);
            Assert.Equal("GotoFirst", response.First().Name);
        }

        [Fact(DisplayName = "No Script Error Should Return Script Result")]
        public async Task Run_Script_Should_Return_Script_Result_When_ScriptError_Zero()
        {
            var mockHttp = CreateMockWithAuth();

            var scriptResponse = System.IO.File.ReadAllText(Path.Combine("ResponseData", "ScriptResponseOK.json"));
            mockHttp.When($"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/script/*")
                    .Respond("application/json", scriptResponse);

            var fdc = GetClient(mockHttp);

            var response = await fdc.RunScriptAsync(Layout, "script-name", null);

            Assert.Equal("Text Based Script Result", response);
        }

        #region Script Result Deserialization Validation

        [Fact(DisplayName = "Full response JSON with dotted script fields deserializes correctly")]
        public void Full_Response_With_Dotted_Script_Fields_Deserializes()
        {
            var json = @"{
                ""response"": {
                    ""recordId"": 10,
                    ""scriptError"": 1,
                    ""scriptResult"": ""post-result"",
                    ""scriptError.prerequest"": 2,
                    ""scriptResult.prerequest"": ""pre-req-result"",
                    ""scriptError.presort"": 3,
                    ""scriptResult.presort"": ""pre-sort-result""
                },
                ""messages"": [{""code"":""0"",""message"":""OK""}]
            }";

            // Deserialize as CreateResponse (simulates real path)
            var response = JsonConvert.DeserializeObject<FMData.Rest.Responses.CreateResponse>(json);

            // Standard fields work via normal deserialization
            Assert.Equal(10, response.Response.RecordId);
            Assert.Equal(1, response.Response.ScriptError);
            Assert.Equal("post-result", response.Response.ScriptResult);

            // Dotted fields require JObject extraction
            var joResponse = JObject.Parse(json);
            var responseToken = joResponse["response"];
            response.Response.ScriptErrorPreRequest = responseToken["scriptError.prerequest"]?.ToObject<int>() ?? 0;
            response.Response.ScriptResultPreRequest = responseToken["scriptResult.prerequest"]?.ToString();
            response.Response.ScriptErrorPreSort = responseToken["scriptError.presort"]?.ToObject<int>() ?? 0;
            response.Response.ScriptResultPreSort = responseToken["scriptResult.presort"]?.ToString();

            Assert.Equal(2, response.Response.ScriptErrorPreRequest);
            Assert.Equal("pre-req-result", response.Response.ScriptResultPreRequest);
            Assert.Equal(3, response.Response.ScriptErrorPreSort);
            Assert.Equal("pre-sort-result", response.Response.ScriptResultPreSort);
        }

        [Fact(DisplayName = "ActionResponse without script fields deserializes with defaults")]
        public void ActionResponse_Without_Script_Fields_Should_Default()
        {
            var json = @"{ ""recordId"": 5, ""modId"": 2 }";

            var result = JsonConvert.DeserializeObject<ActionResponse>(json);

            Assert.Equal(5, result.RecordId);
            Assert.Equal(2, result.ModId);
            Assert.Equal(0, result.ScriptError);
            Assert.Null(result.ScriptResult);
            Assert.Equal(0, result.ScriptErrorPreRequest);
            Assert.Null(result.ScriptResultPreRequest);
            Assert.Equal(0, result.ScriptErrorPreSort);
            Assert.Null(result.ScriptResultPreSort);
        }

        [Fact(DisplayName = "ActionResponse with only post-request script fields works")]
        public void ActionResponse_With_Partial_Script_Fields_Should_Work()
        {
            var json = @"{
                ""scriptError"": 0,
                ""scriptResult"": ""only-post""
            }";

            var result = JsonConvert.DeserializeObject<ActionResponse>(json);

            Assert.Equal(0, result.ScriptError);
            Assert.Equal("only-post", result.ScriptResult);
            Assert.Equal(0, result.ScriptErrorPreRequest);
            Assert.Null(result.ScriptResultPreRequest);
            Assert.Equal(0, result.ScriptErrorPreSort);
            Assert.Null(result.ScriptResultPreSort);
        }

        #endregion

        #region Create Script Result Tests

        [Fact(DisplayName = "Create with all script results returns all fields")]
        public async Task Create_With_All_Script_Results()
        {
            var mockHttp = CreateMockWithAuth();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records*")
                    .Respond("application/json", DataApiResponses.SuccessfulCreateWithScript());

            var fdc = GetClient(mockHttp);

            var response = await fdc.CreateAsync(Layout, new User { Name = "test" });

            Assert.NotNull(response.Response);
            Assert.Equal("create-script-result", response.Response.ScriptResult);
            Assert.Equal(0, response.Response.ScriptError);
            Assert.Equal("create-prerequest-result", response.Response.ScriptResultPreRequest);
            Assert.Equal(0, response.Response.ScriptErrorPreRequest);
            Assert.Equal("create-presort-result", response.Response.ScriptResultPreSort);
            Assert.Equal(0, response.Response.ScriptErrorPreSort);
        }

        [Fact(DisplayName = "Create with script error captures error codes")]
        public async Task Create_With_Script_Error()
        {
            var mockHttp = CreateMockWithAuth();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records*")
                    .Respond("application/json", DataApiResponses.SuccessfulCreateWithScriptError());

            var fdc = GetClient(mockHttp);

            var response = await fdc.CreateAsync(Layout, new User { Name = "test" });

            Assert.NotNull(response.Response);
            Assert.Equal(3, response.Response.ScriptError);
            Assert.Equal(5, response.Response.ScriptErrorPreRequest);
            Assert.Equal(7, response.Response.ScriptErrorPreSort);
        }

        #endregion

        #region Edit Script Result Tests

        [Fact(DisplayName = "Edit with all script results returns all fields")]
        public async Task Edit_With_All_Script_Results()
        {
            var mockHttp = CreateMockWithAuth();

            mockHttp.When(new HttpMethod("PATCH"), $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records/*")
                    .Respond("application/json", DataApiResponses.SuccessfulEditWithScript());

            var fdc = GetClient(mockHttp);

            var response = await fdc.EditAsync(Layout, 1, new User { Name = "test" });

            Assert.NotNull(response.Response);
            Assert.Equal("edit-script-result", response.Response.ScriptResult);
            Assert.Equal(0, response.Response.ScriptError);
            Assert.Equal("edit-prerequest-result", response.Response.ScriptResultPreRequest);
            Assert.Equal(0, response.Response.ScriptErrorPreRequest);
            Assert.Equal("edit-presort-result", response.Response.ScriptResultPreSort);
            Assert.Equal(0, response.Response.ScriptErrorPreSort);
        }

        #endregion

        #region Delete Script Result Tests

        [Fact(DisplayName = "Delete returns IDeleteResponse with script results")]
        public async Task Delete_With_Script_Results()
        {
            var mockHttp = CreateMockWithAuth();

            mockHttp.When(HttpMethod.Delete, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records/*")
                    .Respond("application/json", DataApiResponses.SuccessfulDeleteWithScript());

            var fdc = GetClient(mockHttp);

            var response = await fdc.DeleteAsync(1, Layout);

            Assert.NotNull(response);
            Assert.NotNull(response.Response);
            Assert.Equal("delete-script-result", response.Response.ScriptResult);
            Assert.Equal(0, response.Response.ScriptError);
            Assert.Equal("delete-prerequest-result", response.Response.ScriptResultPreRequest);
            Assert.Equal(0, response.Response.ScriptErrorPreRequest);
            Assert.Equal("delete-presort-result", response.Response.ScriptResultPreSort);
            Assert.Equal(0, response.Response.ScriptErrorPreSort);
        }

        [Fact(DisplayName = "Delete 404 returns null Response")]
        public async Task Delete_404_Returns_Null_Response()
        {
            var mockHttp = CreateMockWithAuth();

            mockHttp.Fallback.Respond(HttpStatusCode.NotFound);

            var fdc = GetClient(mockHttp);

            var response = await fdc.DeleteAsync(999, Layout);

            Assert.NotNull(response);
            Assert.Null(response.Response);
            Assert.Contains(response.Messages, r => r.Code == "404");
        }

        [Fact(DisplayName = "Delete without script fields has null Response")]
        public async Task Delete_Without_Script_Fields()
        {
            var mockHttp = CreateMockWithAuth();

            mockHttp.When(HttpMethod.Delete, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records/*")
                    .Respond("application/json", DataApiResponses.SuccessfulDelete());

            var fdc = GetClient(mockHttp);

            var response = await fdc.DeleteAsync(1, Layout);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        #endregion

        #region Find Script Result Tests

        [Fact(DisplayName = "Find with script results via SendFindRequestAsync")]
        public async Task Find_With_Script_Results_Via_SendFindRequestAsync()
        {
            var mockHttp = CreateMockWithAuth();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/_find")
                    .Respond("application/json", DataApiResponses.SuccessfulFindWithScript());

            var fdc = GetClient(mockHttp);

            var req = new FindRequest<User>() { Layout = Layout };
            req.AddQuery(new User { Id = 4 }, false);

            var (data, info, scriptResponse) = await fdc.SendFindRequestAsync<User, User>(req, null, null);

            Assert.NotEmpty(data);
            Assert.NotNull(scriptResponse);
            Assert.Equal("find-script-result", scriptResponse.ScriptResult);
            Assert.Equal(0, scriptResponse.ScriptError);
            Assert.Equal("find-prerequest-result", scriptResponse.ScriptResultPreRequest);
            Assert.Equal(0, scriptResponse.ScriptErrorPreRequest);
            Assert.Equal("find-presort-result", scriptResponse.ScriptResultPreSort);
            Assert.Equal(0, scriptResponse.ScriptErrorPreSort);
        }

        [Fact(DisplayName = "Find with script results via SendAsync includeDataInfo")]
        public async Task Find_With_Script_Results_Via_SendAsync_IncludeDataInfo()
        {
            var mockHttp = CreateMockWithAuth();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/_find")
                    .Respond("application/json", DataApiResponses.SuccessfulFindWithScript());

            var fdc = GetClient(mockHttp);

            var req = new FindRequest<User>() { Layout = Layout };
            req.AddQuery(new User { Id = 4 }, false);

            var (data, info, scriptResponse) = await fdc.SendAsync(req, true);

            Assert.NotEmpty(data);
            Assert.NotNull(scriptResponse);
            Assert.Equal("find-script-result", scriptResponse.ScriptResult);
            Assert.Equal("find-prerequest-result", scriptResponse.ScriptResultPreRequest);
            Assert.Equal("find-presort-result", scriptResponse.ScriptResultPreSort);
        }

        [Fact(DisplayName = "Find 401 no records returns null ActionResponse")]
        public async Task Find_401_Returns_Null_ActionResponse()
        {
            var mockHttp = CreateMockWithAuth();

            var userLayout = FileMakerRestClient.GetLayoutName(new User());

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{userLayout}/_find")
                    .Respond(HttpStatusCode.InternalServerError, "application/json", DataApiResponses.FindNotFound());

            var fdc = GetClient(mockHttp);

            var req = new FindRequest<User>() { Layout = userLayout };
            req.AddQuery(new User { Id = 999 }, false);

            var (data, info, scriptResponse) = await fdc.SendAsync(req, true);

            Assert.Empty(data);
            Assert.Null(scriptResponse);
        }

        [Fact(DisplayName = "Find 404 returns null ActionResponse")]
        public async Task Find_404_Returns_Null_ActionResponse()
        {
            var mockHttp = CreateMockWithAuth();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/_find")
                    .Respond(HttpStatusCode.NotFound);

            var fdc = GetClient(mockHttp);

            var req = new FindRequest<User>() { Layout = Layout };
            req.AddQuery(new User { Id = 999 }, false);

            var (data, info, scriptResponse) = await fdc.SendAsync(req, true);

            Assert.Empty(data);
            Assert.Null(scriptResponse);
        }

        [Fact(DisplayName = "Empty find (GET records) with script results")]
        public async Task Empty_Find_GET_Records_With_Script_Results()
        {
            var mockHttp = CreateMockWithAuth();

            mockHttp.When(HttpMethod.Get, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records*")
                    .Respond("application/json", DataApiResponses.SuccessfulFindWithScript());

            var fdc = GetClient(mockHttp);

            var req = new FindRequest<User>() { Layout = Layout };
            // No query added — triggers GET records endpoint

            var (data, info, scriptResponse) = await fdc.SendAsync(req, true);

            Assert.NotEmpty(data);
            Assert.NotNull(scriptResponse);
            Assert.Equal("find-script-result", scriptResponse.ScriptResult);
        }

        [Fact(DisplayName = "Find without script fields in response returns zero/null defaults")]
        public async Task Find_Without_Script_Fields_Returns_Defaults()
        {
            var mockHttp = CreateMockWithAuth();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/_find")
                    .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = GetClient(mockHttp);

            var req = new FindRequest<User>() { Layout = Layout };
            req.AddQuery(new User { Id = 4 }, false);

            var (data, info, scriptResponse) = await fdc.SendFindRequestAsync<User, User>(req, null, null);

            Assert.NotEmpty(data);
            Assert.NotNull(scriptResponse);
            Assert.Equal(0, scriptResponse.ScriptError);
            Assert.Null(scriptResponse.ScriptResult);
            Assert.Equal(0, scriptResponse.ScriptErrorPreRequest);
            Assert.Null(scriptResponse.ScriptResultPreRequest);
            Assert.Equal(0, scriptResponse.ScriptErrorPreSort);
            Assert.Null(scriptResponse.ScriptResultPreSort);
        }

        #endregion
    }
}
