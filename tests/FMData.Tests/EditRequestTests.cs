using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FMData.Requests;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Tests
{
    public class EditRequestTests
    {
        [Fact]
        public async Task EditShould_UpdateRecord_WithId()
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
                .Respond("application/json", DataApiResponses.SuccessfulEdit());

            using (var fdc = new FMDataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout))
            {
                var req = new EditRequest<Dictionary<string, string>>()
                {
                    Layout = "layout",
                    RecordId = "264",
                    Data = new Dictionary<string, string>()
                    {
                        { "Name", "Fuzzerd-Updated" },
                        { "AnotherField", "Another-Updated" }
                    }
                };
                var response = await fdc.EditAsync(req);

                Assert.NotNull(response);
                Assert.Equal("OK", response.Result);
            }
        }
    }
}