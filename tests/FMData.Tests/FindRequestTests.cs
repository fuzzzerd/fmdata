using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FMData.Requests;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Tests
{
    public class FindRequestTests
    {
        [Fact]
        public async Task FindShould_ReturnData()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "layout";

            mockHttp.When($"{server}/fmi/rest/api/auth/{file}")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When($"{server}/fmi/rest/api/find/{file}/{layout}")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FMDataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);

            var response = await fdc.FindAsync(new FindRequest()
            {
                Query = new List<Dictionary<string, string>>() {
                    new Dictionary<string,string>()
                    {
                        {"Name","Bross"}
                    },
                    new Dictionary<string,string>()
                    {
                        {"Name","Admin"}, {"omit","true"},
                    }
                }, Layout = layout
            });

            var responseDataContainsResult = response.Data.Any(r => r.FieldData.Any(v => v.Value.Contains("Bross")));

            Assert.True(responseDataContainsResult);
        }
    }
}