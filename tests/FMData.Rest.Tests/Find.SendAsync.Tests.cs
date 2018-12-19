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
        [Fact]
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
        public async Task SendAsync_FindWithoutQuery_ShouldConvertToGetRange_AndReturnMany()
        {
            var fdc = FindTestsHelpers.GetMockedFDC();

            var response = await fdc.SendAsync(new FindRequest<Dictionary<string, string>>() { Layout = "layout" });

            Assert.Equal(2, response.Response.Data.Count());
        }

        [Fact]
        public async Task SendAsync_FindWithoutlayout_ShouldThrowArgumentException()
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

            // assert (any becuase our data is mixed and has both)
            var responseDataContainsResult = response.Any(r => r.FileMakerModId != 0);
            Assert.True(responseDataContainsResult);
        }

        [Fact]
        public async Task SendAsyncFind_WithBadLayout_Throws()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.server}/fmi/data/v1/databases/{FindTestsHelpers.file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.server}/fmi/data/v1/databases/{FindTestsHelpers.file}/layouts/*")
                .Respond(HttpStatusCode.InternalServerError, "application/json", DataApiResponses.LayoutNotFound());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(),
                FindTestsHelpers.server,
                FindTestsHelpers.file,
                FindTestsHelpers.user,
                FindTestsHelpers.pass);

            // act
            // assert
            await Assert.ThrowsAsync<Exception>(async () => await fdc.SendAsync(FindTestsHelpers.FindUserReqWithLayoutOverride()));
        }

        [Fact]
        public async Task SendAsyncFind_Record_ThatDoesNotExist_ShouldReturnEmpty()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var layout = FileMakerRestClient.GetLayoutName(new User());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.server}/fmi/data/v1/databases/{FindTestsHelpers.file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.server}/fmi/data/v1/databases/{FindTestsHelpers.file}/layouts/{layout}/_find")
                .Respond(HttpStatusCode.InternalServerError, "application/json", DataApiResponses.FindNotFound());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(),
                FindTestsHelpers.server,
                FindTestsHelpers.file,
                FindTestsHelpers.user,
                FindTestsHelpers.pass);

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

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.server}/fmi/data/v1/databases/{FindTestsHelpers.file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.server}/fmi/data/v1/databases/{FindTestsHelpers.file}/layouts/{layout}/_find")
                .Respond(HttpStatusCode.InternalServerError, "application/json", DataApiResponses.FindNotFound());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(),
                FindTestsHelpers.server,
                FindTestsHelpers.file,
                FindTestsHelpers.user,
                FindTestsHelpers.pass);

            // act
            var toFind = new User() { Id = 35 };

            // assert
            var req = new FindRequest<User>() { };
            req.AddQuery(toFind, false);
            await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.SendAsync(req));
        }

        [Fact]
        public async Task SendAsync_Dictionary_WithPortals_ShouldHaveData()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.server}/fmi/data/v1/databases/{FindTestsHelpers.file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.server}/fmi/data/v1/databases/{FindTestsHelpers.file}/layouts/{layout}/_find")
                .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFindWithPortal());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(),
                FindTestsHelpers.server,
                FindTestsHelpers.file,
                FindTestsHelpers.user,
                FindTestsHelpers.pass);

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
        public async Task SendAsyncFind_WithOmit_Omits()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();
            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.server}/fmi/data/v1/databases/{FindTestsHelpers.file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.server}/fmi/data/v1/databases/{FindTestsHelpers.file}/layouts/*")
                .WithPartialContent("omit")
                .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(),
                FindTestsHelpers.server,
                FindTestsHelpers.file,
                FindTestsHelpers.user,
                FindTestsHelpers.pass);

            var toFind = new User() { Id = 35 };
            var req = new FindRequest<User>() { Layout = layout };
            req.AddQuery(toFind, true);
            var response = await fdc.SendAsync(req);

            // assert
            // since we're not really talking to fms, we know our 
            // response data has 4, so we know if we had an 'omit'
            // in our request and a 4 in our resposne things worked as expected.
            Assert.Contains(response, c => c.Id == 4);
        }
    }
}