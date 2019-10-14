using RichardSzalay.MockHttp;
using System;
using System.Net.Http;
using System.Threading.Tasks;
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

            var productInfo = System.IO.File.ReadAllText("ResponseData\\ProductInfo.xml");
            mockHttp.When($"{server}/fmi/xml/fmresultset.xml")
               .Respond("application/json", productInfo);

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
        }
    }
}