using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Xml.Tests
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

            var productInfo = System.IO.File.ReadAllText(Path.Combine("ResponseData", "ProductInfo.xml"));
            mockHttp.When($"{server}/fmi/xml/fmresultset.xml")
                    .Respond("application/xml", productInfo);

            var fdc = new FileMakerXmlClient(mockHttp.ToHttpClient(),
                new ConnectionInfo
                {
                    FmsUri = server,
                    Database = file,
                    Username = user,
                    Password = pass
                });

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

            var productInfo = System.IO.File.ReadAllText(Path.Combine("ResponseData", "Databases.xml"));
            mockHttp.When($"{server}/fmi/xml/fmresultset.xml?-dbnames")
                    .Respond("application/xml", productInfo);

            var fdc = new FileMakerXmlClient(mockHttp.ToHttpClient(), new ConnectionInfo { FmsUri = server, Database = file, Username = user, Password = pass });

            var response = await fdc.GetDatabasesAsync();

            Assert.NotNull(response);
            Assert.Equal("Database1", response.First());
        }
    }
}
