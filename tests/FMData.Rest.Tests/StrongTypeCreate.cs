using FMData.Rest;
using RichardSzalay.MockHttp;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FMData.Tests
{
    public class StrongTypeCreate
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

        private static IFileMakerApiClient GetMockedFDC()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "layout";

            mockHttp.When($"{server}/fmi/rest/api/auth/{file}")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/rest/api/record/{file}/*")
                .WithPartialContent("data") // make sure that the body content contains the 'data' object expected by fms
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

            var fdc = new DataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);
            return fdc;
        }

        [Fact]
        public async Task StrongCreate_Should_ReturnDataFromCreation()
        {
            var fdc = GetMockedFDC();

            var newModel = new TableModelTest()
            {
                Name = "Fuzzzerd",
                AnotherField = "Different Value"
            };

            var response = await fdc.CreateAsync(newModel);

            Assert.NotNull(response);
            Assert.Equal("OK", response.Result);
        }

        [Fact]
        public async Task StrongCreateSpecificLayout_Should_ReturnDataFromCreation()
        {
            var fdc = GetMockedFDC();

            var newModel = new ModelTest()
            {
                Name = "Fuzzzerd",
                AnotherField = "Different Value"
            };

            var response = await fdc.CreateAsync("layout", newModel);

            Assert.NotNull(response);
            Assert.Equal("OK", response.Result);
        }

        [Fact]
        public async Task Create_TablelessModel_Should_Throw_ArgumentException()
        {
            var fdc = GetMockedFDC();

            var newModel = new ModelTest()
            {
                Name = "Fuzzzerd",
                AnotherField = "Different Value"
            };

            await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.CreateAsync(newModel));
        }

    }
}