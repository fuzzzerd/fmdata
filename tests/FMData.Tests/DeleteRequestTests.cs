using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FMData.Requests;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Tests
{
    public class DeleteRequestTests
    {
        [Fact]
        public async Task DeleteShould_NukeRecordId()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "layout";

            mockHttp.When($"{server}/fmi/rest/api/auth/{file}")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When($"{server}/fmi/rest/api/record/{file}/{layout}/*")
                .Respond("application/json", DataApiResponses.SuccessfulDelete());

            using (var fdc = new FMDataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout))
            {

                var req = new DeleteRequest()
                {
                    Layout = "layout",
                    RecordId = "1234"
                };
                var response = await fdc.DeleteRecord(req);

                Assert.NotNull(response);
                Assert.NotNull(response.RecordId);
                Assert.True(response.RecordId != "0");
            }
        }
    }
}