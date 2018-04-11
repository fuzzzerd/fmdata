using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FMData.Requests;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Tests
{
    public class CreateRequestTests
    {
        [Fact(DisplayName = "Create should generate a new record id.")]
        public async Task CreateShould_ReturnRecordId()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "layout";

            mockHttp.When($"{server}/fmi/rest/api/auth/{file}")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When($"{server}/fmi/rest/api/record/{file}/{layout}")
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

            var fdc = new FMDataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);

            var req = new CreateRequest()
            {
                Layout = "layout",
                Data = new Dictionary<string, string>()
                {
                    { "Name", "Fuzzerd" },
                    { "AnotherField", "Another Valuee" }
                }
            };
            var response = await fdc.CreateRecord(req);

            Assert.NotNull(response);
            Assert.NotNull(response.RecordId);
            Assert.True(response.RecordId != "0");
        }
    }
}