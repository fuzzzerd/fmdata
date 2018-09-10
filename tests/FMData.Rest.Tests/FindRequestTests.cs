using FMData.Rest;
using FMData.Rest.Requests;
using FMData.Rest.Tests.TestModels;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FMData.Rest.Tests
{
    public class FindRequestTests
    {
        private static IFileMakerApiClient GetMockedFDC()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
               .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Get, $"{server}/fmi/data/v1/databases/{file}/layout*")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/layout/_find")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/Users/_find")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);
            return fdc;
        }

        private IFindRequest<Dictionary<string, string>> FindReq => new FindRequest<Dictionary<string, string>>()
        {
            Query = new List<Dictionary<string, string>>()
            {
                new Dictionary<string,string>()
                {
                    {"Name","fuzzzerd"}
                },
                new Dictionary<string,string>()
                {
                    {"Name","Admin"}, {"omit","true"},
                }
            },
            Layout = "layout"
        };

        private IFindRequest<User> FindUserReqWithLayoutOverride => new FindRequest<User>()
        {
            Query = new List<User>()
            {
                new User()
                {
                    Id =1
                }
            },
            Layout = "layout"
        };

        [Fact]
        public async Task SendAsync_Find_Should_ReturnData()
        {
            var fdc = GetMockedFDC();

            var response = await fdc.SendAsync(FindReq);

            var responseDataContainsResult = response.Response.Data.Any(r => r.FieldData.Any(v => v.Value.Contains("Buzz")));

            Assert.True(responseDataContainsResult);
        }

        [Fact]
        public async Task FindAsync_WithSkipTake_ShouldHave_LimitOffset()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "Users";

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/_find")
                .WithPartialContent("limit") // ensure the request contains the expected content
                .WithPartialContent("offset") // ensure the request contains the expected content
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);

            // act
            var response = await fdc.FindAsync(new User()
            {
                Name = "fuzzzerd"
            },5,5);

            // assert
            Assert.NotEmpty(response);
        }

        [Fact]
        public async Task SendAsync_EmptyFind_ShouldReturnMany()
        {
            var fdc = GetMockedFDC();

            var response = await fdc.SendAsync(new FindRequest<User> { Layout = "layout" });

            Assert.Equal(2, response.Count());
        }

        [Fact]
        public async Task SendAsync_FindWithoutQuery_ShouldConvertToGetRange_AndReturnMany()
        {
            var fdc = GetMockedFDC();

            var response = await fdc.SendAsync(new FindRequest<Dictionary<string, string>>() { Layout = "layout" });

            Assert.Equal(2, response.Response.Data.Count());
        }

        [Fact]
        public async Task SendAsync_FindWithoutlayout_ShouldThrowArgumentException()
        {
            var fdc = GetMockedFDC();

            await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.SendAsync(new FindRequest<Dictionary<string, string>>() { }));
        }

        [Fact]
        public async Task SendAsync_StrongType_FindShould_ReturnData()
        {
            // arrange
            var fdc = GetMockedFDC();

            // act
            var response = await fdc.SendAsync(FindUserReqWithLayoutOverride);

            // assert
            var responseDataContainsResult = response.Any(r => r.Name.Contains("Buzz"));
            Assert.True(responseDataContainsResult);
        }

        [Fact]
        public async Task SendAsync_StrongType_FindShould_ReturnData_AndGetFMID()
        {
            // arrange
            var fdc = GetMockedFDC();

            // act
            var response = await fdc.SendAsync(FindUserReqWithLayoutOverride, (u, i) => u.FileMakerRecordId = i);

            // assert
            var responseDataContainsResult = response.All(r => r.FileMakerRecordId != 0);
            Assert.True(responseDataContainsResult);
        }

        [Fact]
        public async Task SendAsync_StrongType_FindShould_ReturnData_AndGetModID()
        {
            // arrange
            var fdc = GetMockedFDC();

            // act
            var response = await fdc.SendAsync(FindUserReqWithLayoutOverride, null, (u, i) => u.FileMakerModId = i);

            // assert (any becuase our data is mixed and has both)
            var responseDataContainsResult = response.Any(r => r.FileMakerModId != 0);
            Assert.True(responseDataContainsResult);
        }

        [Fact]
        public async Task Test_DateTime_To_Timestamp_Parsing()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "Users";

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/_find")
                .WithPartialContent("fuzzzerd") // ensure the request contains the expected content
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);

            // act
            var response = await fdc.FindAsync(new User()
            {
                Name = "fuzzzerd"
            });

            // assert
            var responseDataContainsResult = response.Any(r => r.Created == DateTime.Parse("03/29/2018 15:22:09"));
            Assert.True(responseDataContainsResult);
        }

        [Fact]
        public async Task Find_WithScript_ShouldHaveScript()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "Users";

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/_find")
                .WithPartialContent("fuzzzerd") // ensure the request contains the expected content
                .WithPartialContent("script").WithPartialContent("nos_ran")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);

            // act
            var response = await fdc.FindAsync(new User()
            {
                Name = "fuzzzerd"
            }, "nos_ran", null, null);

            // assert
            var responseDataContainsResult = response.Any(r => r.Created == DateTime.Parse("03/29/2018 15:22:09"));
            Assert.True(responseDataContainsResult);
        }

        [Fact]
        public async Task SendAsyncFind_WithBadLayout_Throws()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/*")
                .Respond(HttpStatusCode.InternalServerError, "application/json", DataApiResponses.LayoutNotFound());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);

            // act
            // assert
            await Assert.ThrowsAsync<Exception>(async () => await fdc.SendAsync(FindUserReqWithLayoutOverride));
        }

        [Fact]
        public async Task Find_NotFound_Should_ReturnEmpty()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = FileMakerRestClient.GetTableName(new User());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/_find")
                .Respond(HttpStatusCode.InternalServerError, "application/json", DataApiResponses.FindNotFound());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);

            // act
            var toFind = new User() { Id = 35 };
            var response = await fdc.FindAsync(toFind);

            // assert
            Assert.Empty(response);
        }

        [Fact]
        public async Task SendAsyncFind_Record_ThatDoesNotExist_ShouldReturnEmpty()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = FileMakerRestClient.GetTableName(new User());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/_find")
                .Respond(HttpStatusCode.InternalServerError, "application/json", DataApiResponses.FindNotFound());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);

            // act
            var toFind = new User() { Id = 35 };
            var response = await fdc.SendAsync(new FindRequest<User>() { Query = new List<User> { toFind }, Layout = layout });

            // assert
            Assert.Empty(response);
        }


        [Fact]
        public async Task SendAsyncFind_WithoutLayout_ShouldThrow()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = FileMakerRestClient.GetTableName(new User());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/_find")
                .Respond(HttpStatusCode.InternalServerError, "application/json", DataApiResponses.FindNotFound());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);

            // act
            var toFind = new User() { Id = 35 };

            // assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.SendAsync(new FindRequest<User>() { Query = new List<User> { toFind } }));
        }

        [Fact]
        public async Task GetByRecordId_ShouldReturnMatchingRecordId()
        {
            // arrange
            object FMrecordIdMapper(User o, int id) => o.FileMakerRecordId = id;
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "Users";
            var recordId = 26;

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Get, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records/{recordId}")
                .Respond("application/json", DataApiResponses.SuccessfulGetById(recordId));

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);

            // act
            var response = await fdc.GetByFileMakerIdAsync<User>(layout, recordId, FMrecordIdMapper);

            // assert
            Assert.Equal(recordId, response.FileMakerRecordId);
        }

        [Fact]
        public async Task GetByRecordId_ShouldHaveContainerWithContainerDataFor()
        {
            // arrange
            object FMrecordIdMapper(ContainerFieldTestModel o, int id) => o.FileMakerRecordId = id;
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "Users";
            var recordId = 26;

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var b64String = System.IO.File.ReadAllText("ResponseData\\b64-string.dat");
            var bytes = Convert.FromBase64String(b64String);
            var b64= new ByteArrayContent(bytes);

            mockHttp.When(HttpMethod.Get, $"{server}/some-data-path")
                .Respond(b64);

            mockHttp.When(HttpMethod.Get, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records/{recordId}")
                .Respond("application/json", DataApiResponses.SuccessfulGetByIdWithContainer(recordId, $"{server}/some-data-path"));

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);

            // act
            var response = await fdc.GetByFileMakerIdAsync<ContainerFieldTestModel>(recordId, FMrecordIdMapper);

            // assert
            Assert.Equal(bytes, response.SomeContainerFieldData);
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


        [Fact]
        public async Task SendAsync_Dictionary_WithPortals_ShouldHaveData()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/_find")
                .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFindWithPortal());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);

            var fr = new FindRequest<Dictionary<string, string>> {
                Layout = layout,
                Query = new List<Dictionary<string, string>>
                {
                    new Dictionary<string, string> { { "one", "one" } }
                }
            };

            // act
            var response = await fdc.SendAsync(fr);

            // assert
            Assert.NotEmpty(response.Response.Data);
        }
    }
}