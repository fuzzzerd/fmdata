using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FMData.Rest.Requests;
using FMData.Rest.Tests.TestModels;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Rest.Tests
{
    public class FindRequestTests
    {
        [Fact]
        [Obsolete]
        public async Task SendAsync_Find_Should_ReturnData()
        {
            var fdc = FindTestsHelpers.GetMockedFDC();

            var response = await fdc.SendAsync(FindTestsHelpers.FindReq());

            var responseDataContainsResult = response.Response.Data.Any(r => r.FieldData.Any(v => v.Value.Contains("Buzz")));

            Assert.True(responseDataContainsResult);
        }

        [Fact]
        public async Task SendAsync_EmptyFind_ShouldReturnMany()
        {
            var fdc = FindTestsHelpers.GetMockedFDC();

            var response = await fdc.SendAsync(new FindRequest<User> { Layout = "layout" });

            Assert.Equal(2, response.Count());
        }

        [Fact]
        public async Task SendAsync_Using_Var_With_Mappers_ShouldReturnMany()
        {
            var fdc = FindTestsHelpers.GetMockedFDC();
            Func<User, int, object> fMRecordIdMapper = (o, id) => o.FileMakerRecordId = id;
            Func<User, int, object> fMModIdMapper = (o, id) => o.FileMakerRecordId = id;

            var response = await fdc.SendAsync(new FindRequest<User> { Layout = "layout" }, fMRecordIdMapper, fMModIdMapper);

            Assert.Equal(2, response.Count());
        }

        [Fact]
        public async Task SendAsync_Explicit_Type_With_Mappers_ShouldReturnMany()
        {
            var fdc = FindTestsHelpers.GetMockedFDC();
            Func<User, int, object> fMRecordIdMapper = (o, id) => o.FileMakerRecordId = id;
            Func<User, int, object> fMModIdMapper = (o, id) => o.FileMakerRecordId = id;

            IEnumerable<User> response;

            response = await fdc.SendAsync(new FindRequest<User> { Layout = "layout" }, fMRecordIdMapper, fMModIdMapper);

            Assert.Equal(2, response.Count());
        }

        [Fact]
        [Obsolete]
        public async Task SendAsync_FindWithoutQuery_ShouldConvertToGetRange_AndReturnMany()
        {
            var fdc = FindTestsHelpers.GetMockedFDC();

            var response = await fdc.SendAsync(new FindRequest<Dictionary<string, string>>() { Layout = "layout" });

            Assert.Equal(2, response.Response.Data.Count());
        }

        [Fact]
        [Obsolete]
        public async Task SendAsync_FindWithoutLayout_ShouldThrowArgumentException()
        {
            var fdc = FindTestsHelpers.GetMockedFDC();

            await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.SendAsync(new FindRequest<Dictionary<string, string>>() { }));
        }

        [Fact]
        public async Task SendAsync_StrongType_FindShould_ReturnData()
        {
            // arrange
            var fdc = FindTestsHelpers.GetMockedFDC();

            // act
            var response = await fdc.SendAsync(FindTestsHelpers.FindUserReqWithLayoutOverride());

            // assert
            var responseDataContainsResult = response.Any(r => r.Name.Contains("Buzz"));
            Assert.True(responseDataContainsResult);
        }

        [Fact]
        public async Task SendAsync_StrongType_FindShould_ReturnData_AndGetFMID()
        {
            // arrange
            var fdc = FindTestsHelpers.GetMockedFDC();

            // act
            var response = await fdc.SendAsync(FindTestsHelpers.FindUserReqWithLayoutOverride(), (u, i) => u.FileMakerRecordId = i);

            // assert
            var responseDataContainsResult = response.All(r => r.FileMakerRecordId != 0);
            Assert.True(responseDataContainsResult);
        }

        [Fact]
        public async Task SendAsync_StrongType_FindShould_ReturnData_AndGetModID()
        {
            // arrange
            var fdc = FindTestsHelpers.GetMockedFDC();

            // act
            var response = await fdc.SendAsync(FindTestsHelpers.FindUserReqWithLayoutOverride(), null, (u, i) => u.FileMakerModId = i);

            // assert (any because our data is mixed and has both)
            var responseDataContainsResult = response.Any(r => r.FileMakerModId != 0);
            Assert.True(responseDataContainsResult);
        }

        [Fact]
        public async Task SendAsyncFind_WithBadLayout_Throws()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/*")
                    .Respond(HttpStatusCode.InternalServerError, "application/json", DataApiResponses.LayoutNotFound());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            // act
            // assert
            await Assert.ThrowsAsync<FMDataException>(async () => await fdc.SendAsync(FindTestsHelpers.FindUserReqWithLayoutOverride()));
        }

        [Fact]
        public async Task SendAsyncFind_Record_ThatDoesNotExist_ShouldReturnEmpty()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var layout = FileMakerRestClient.GetLayoutName(new User());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/{layout}/_find")
                    .Respond(HttpStatusCode.InternalServerError, "application/json", DataApiResponses.FindNotFound());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            // act
            var toFind = new User() { Id = 35 };
            var req = new FindRequest<User>() { Layout = layout };
            req.AddQuery(toFind, false);
            var response = await fdc.SendAsync(req);

            // assert
            Assert.Empty(response);
        }

        [Fact]
        public async Task SendAsyncFind_WithoutLayout_ShouldThrow()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var layout = FileMakerRestClient.GetLayoutName(new User());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/{layout}/_find")
                    .Respond(HttpStatusCode.InternalServerError, "application/json", DataApiResponses.FindNotFound());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            // act
            var toFind = new User() { Id = 35 };

            // assert
            var req = new FindRequest<User>() { };
            req.AddQuery(toFind, false);
            await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.SendAsync(req));
        }

        [Fact]
        [Obsolete]
        public async Task SendAsync_Dictionary_WithPortals_ShouldHaveData()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/{layout}/_find")
                    .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFindWithPortal());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var fr = new FindRequest<Dictionary<string, string>>
            {
                Layout = layout
            };
            fr.AddQuery(new Dictionary<string, string> { { "one", "one" } }, false);

            // act
            var response = await fdc.SendAsync(fr);

            // assert
            Assert.NotEmpty(response.Response.Data);
        }

        [Fact]
        public async Task SendAsync_Find_Should_Have_DataInfo()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/{layout}/_find")
                    .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFindWithDataInfo());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var toFind = new User() { Id = 35 };
            var req = new FindRequest<User>() { Layout = layout };
            req.AddQuery(toFind, false);

            // act
            var (data, info, _) = await fdc.SendAsync(req, true);

            // assert
            Assert.NotEmpty(data);
            Assert.Equal(1, info.ReturnedCount);
            Assert.Equal(123, info.FoundCount);
        }

        [Fact]
        public async Task SendAsync_Find_Should_Have_DataInfo_FirstOverload()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();
            Func<User, int, object> FMRecordIdMapper = (o, id) => o.FileMakerRecordId = id;

            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/{layout}/_find")
                    .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFindWithDataInfo());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var toFind = new User() { Id = 35 };
            var req = new FindRequest<User>() { Layout = layout };
            req.AddQuery(toFind, false);

            // act
            var (data, info, _) = await fdc.SendAsync(req, true, FMRecordIdMapper, null);

            // assert
            Assert.NotEmpty(data);
            Assert.Equal(1, info.ReturnedCount);
            Assert.Equal(123, info.FoundCount);
        }

        [Fact]
        public async Task SendAsync_Find_Should_Have_DataInfo_SecondOverload()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();
            Func<User, int, object> ModMap = (o, id) => o.FileMakerModId = id;

            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/{layout}/_find")
                    .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFindWithDataInfo());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var toFind = new User() { Id = 35 };
            var req = new FindRequest<User>() { Layout = layout };
            req.AddQuery(toFind, false);

            // act
            var (data, info, _) = await fdc.SendAsync(req, true, null, ModMap);

            // assert
            Assert.NotEmpty(data);
            Assert.Equal(1, info.ReturnedCount);
            Assert.Equal(123, info.FoundCount);
        }

        [Fact]
        public async Task SendAsync_Using_Dictionary_Find_Should_Have_DataInfo()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/{layout}/_find")
                    .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFindWithDataInfo());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var toFind = new Dictionary<string, string>() { { "Id", "35" } };
            var req = new FindRequest<Dictionary<string, string>>() { Layout = layout };
            req.AddQuery(toFind, false);

            // act
            var (data, info, _) = await fdc.SendFindRequestAsync<User, Dictionary<string, string>>(req, null, null);

            // assert
            Assert.NotEmpty(data);
            Assert.Equal(1, info.ReturnedCount);
            Assert.Equal(123, info.FoundCount);
        }

        [Fact]
        public async Task SendAsync_Using_Dictionary_Find_Should_Have_DataInfo_OverloadOne()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();
            Func<User, int, object> IdMap = (o, id) => o.FileMakerRecordId = id;
            Func<User, int, object> ModMap = (o, id) => o.FileMakerModId = id;

            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/{layout}/_find")
                    .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFindWithDataInfo());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var toFind = new Dictionary<string, string>() { { "Id", "35" } };
            var req = new FindRequest<Dictionary<string, string>>() { Layout = layout };
            req.AddQuery(toFind, false);

            // act
            var (data, info, _) = await fdc.SendFindRequestAsync<User, Dictionary<string, string>>(req, IdMap, null);

            // assert
            Assert.NotEmpty(data);
            Assert.Equal(1, info.ReturnedCount);
            Assert.Equal(123, info.FoundCount);
        }

        [Fact]
        public async Task SendAsync_Using_Dictionary_Find_Should_Have_DataInfo_OverloadTwo()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();
            Func<User, int, object> IdMap = (o, id) => o.FileMakerRecordId = id;
            Func<User, int, object> ModMap = (o, id) => o.FileMakerModId = id;

            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/{layout}/_find")
                    .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFindWithDataInfo());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var toFind = new Dictionary<string, string>() { { "Id", "35" } };
            var req = new FindRequest<Dictionary<string, string>>() { Layout = layout };
            req.AddQuery(toFind, false);

            // act
            var (data, info, _) = await fdc.SendFindRequestAsync<User, Dictionary<string, string>>(req, null, ModMap);

            // assert
            Assert.NotEmpty(data);
            Assert.Equal(1, info.ReturnedCount);
            Assert.Equal(123, info.FoundCount);
        }

        [Fact]
        public async Task SendAsync_Using_Dictionary_Find_Should_Have_DataInfo_OverloadThree()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();
            Func<User, int, object> IdMap = (o, id) => o.FileMakerRecordId = id;
            Func<User, int, object> ModMap = (o, id) => o.FileMakerModId = id;

            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/{layout}/_find")
                    .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFindWithDataInfo());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var toFind = new Dictionary<string, string>() { { "Id", "35" } };
            var req = new FindRequest<Dictionary<string, string>>() { Layout = layout };
            req.AddQuery(toFind, false);

            // act
            var (data, info, _) = await fdc.SendFindRequestAsync<User, Dictionary<string, string>>(req, IdMap, ModMap);

            // assert
            Assert.NotEmpty(data);
            Assert.Equal(1, info.ReturnedCount);
            Assert.Equal(123, info.FoundCount);
        }

        [Fact]
        public async Task SendAsyncFind_WithOmit_Omits()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();
            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/*")
                    .WithPartialContent("omit")
                    .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var toFind = new User() { Id = 35 };
            var req = new FindRequest<User>() { Layout = layout };
            req.AddQuery(toFind, true);
            var response = await fdc.SendAsync(req);

            // assert
            // since we're not really talking to fms, we know our 
            // response data has 4, so we know if we had an 'omit'
            // in our request and a 4 in our response things worked as expected.
            Assert.Contains(response, c => c.Id == 4);
        }
    }
}
