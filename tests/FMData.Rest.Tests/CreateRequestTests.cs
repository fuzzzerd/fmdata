using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FMData;
using FMData.Rest;
using FMData.Rest.Requests;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Tests
{
    public class CreateRequestTests
    {
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

        private static IFileMakerApiClient GetMockedFDC(MockHttpMessageHandler mockHttp = null)
        {
            if(mockHttp == null) { mockHttp = new MockHttpMessageHandler(); }

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "layout";

            mockHttp.When($"{server}/fmi/data/v1/databases/{file}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records*")
                .WithPartialContent("fieldData") // make sure that the body content contains the 'data' object expected by fms
                .Respond("application/json", DataApiResponses.SuccessfulDelete());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/Somelayout/records*")
                .WithPartialContent("fieldData") // make sure that the body content contains the 'data' object expected by fms
                .Respond("application/json", DataApiResponses.SuccessfulDelete());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), server, file, user, pass);
            return fdc;
        }

        [Fact]
        public async Task CreateWithTableAttribute_ShouldReturnOK()
        {
            var fdc = GetMockedFDC();

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
            var fdc = GetMockedFDC();

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
        public async Task CreateWithoutTableAttirbute_ShouldThrow_WithoutExplicitLayout()
        {
            var fdc = GetMockedFDC();

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
            IFileMakerApiClient fdc = GetMockedFDC();

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
            IFileMakerApiClient fdc = GetMockedFDC();

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
    }
}