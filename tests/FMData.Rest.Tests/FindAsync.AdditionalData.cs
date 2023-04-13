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
    public class FindAsyncAdditionalData
    {
        [Fact]
        public async Task FindAsync_Should_Handle_Extra_Data()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var layout = "Users";

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/{layout}/_find")
                .WithPartialContent("limit") // ensure the request contains the expected content
                .WithPartialContent("offset") // ensure the request contains the expected content
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            // act
            var response = await fdc.FindAsync(new AdditionalDataUser()
            {
                Name = "fuzzzerd"
            }, 5, 5);
            var user = response.First();

            // assert
            Assert.NotEmpty(user.AdditionalData);
        }
    }
}
