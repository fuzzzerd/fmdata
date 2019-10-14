using FMData.Rest.Tests.TestModels;
using RichardSzalay.MockHttp;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FMData.Rest.Tests
{
    public class MetadataTests
    {
        [Fact(DisplayName = "Product Info Metadata Should Return ProductInformation")]
        public async Task GetProductInfo_Should_Return_OK()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
               .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var productInfo = System.IO.File.ReadAllText("ResponseData\\ProductInfo.json");
            mockHttp.When($"{server}/fmi/data/v1/productinfo")
               .Respond("application/json", productInfo);

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), new ConnectionInfo { FmsUri = server, Database = file, Username = user, Password = pass });

            var response = await fdc.GetProductInformationAsync();

            Assert.NotNull(response);
            Assert.NotEmpty(response.Name);
        }

        [Fact(DisplayName = "Product Database Names Should Return List of Strings")]
        public async Task GetDatabases_Should_Return_OK()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";

            var productInfo = System.IO.File.ReadAllText("ResponseData\\Databases.json");
            mockHttp.When($"{server}/fmi/data/v1/databases")
                .With(r => r.Headers.Authorization.Scheme.Equals("basic", StringComparison.CurrentCultureIgnoreCase))
               .Respond("application/json", productInfo);

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), new ConnectionInfo { FmsUri = server, Database = file, Username = user, Password = pass });

            var response = await fdc.GetDatabasesAsync();

            Assert.NotNull(response);
            Assert.Equal("Database1", response.First());
        }
    }
}