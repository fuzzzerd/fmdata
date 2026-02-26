using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FMData.Rest.Requests;
using FMData.Rest.Tests.TestModels;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Rest.Tests
{
    public class PortalRequestTests
    {
        #region Serialization Tests

        [Fact]
        public void SerializeRequest_WithPortal_ProducesCorrectDotNotationKeys()
        {
            var req = new FindRequest<User> { Layout = "layout" };
            req.AddQuery(new User { Id = 1 }, false);
            req.ConfigurePortal("RelatedItems", limit: 100, offset: 5);

            var json = req.SerializeRequest();
            var jo = JObject.Parse(json);

            Assert.NotNull(jo["portal"]);
            Assert.Contains("RelatedItems", jo["portal"].ToObject<List<string>>());
            Assert.Equal(100, jo["limit.RelatedItems"].Value<int>());
            Assert.Equal(5, jo["offset.RelatedItems"].Value<int>());
        }

        [Fact]
        public void SerializeRequest_WithoutPortals_ProducesUnchangedJson()
        {
            var req = new FindRequest<User> { Layout = "layout" };
            req.AddQuery(new User { Id = 1 }, false);

            var json = req.SerializeRequest();
            var jo = JObject.Parse(json);

            Assert.Null(jo["portal"]);
            Assert.NotNull(jo["query"]);
        }

        [Fact]
        public void SerializeRequest_WithLimitOnly_ProducesLimitWithoutOffset()
        {
            var req = new FindRequest<User> { Layout = "layout" };
            req.AddQuery(new User { Id = 1 }, false);
            req.ConfigurePortal("Orders", limit: 50);

            var json = req.SerializeRequest();
            var jo = JObject.Parse(json);

            Assert.Equal(50, jo["limit.Orders"].Value<int>());
            Assert.Null(jo["offset.Orders"]);
        }

        [Fact]
        public void SerializeRequest_WithOffsetOnly_ProducesOffsetWithoutLimit()
        {
            var req = new FindRequest<User> { Layout = "layout" };
            req.AddQuery(new User { Id = 1 }, false);
            req.ConfigurePortal("Orders", offset: 10);

            var json = req.SerializeRequest();
            var jo = JObject.Parse(json);

            Assert.Null(jo["limit.Orders"]);
            Assert.Equal(10, jo["offset.Orders"].Value<int>());
        }

        [Fact]
        public void SerializeRequest_WithPortalNameOnly_ProducesPortalArrayWithNoLimitOrOffset()
        {
            var req = new FindRequest<User> { Layout = "layout" };
            req.AddQuery(new User { Id = 1 }, false);
            req.ConfigurePortal("Orders");

            var json = req.SerializeRequest();
            var jo = JObject.Parse(json);

            Assert.Contains("Orders", jo["portal"].ToObject<List<string>>());
            Assert.Null(jo["limit.Orders"]);
            Assert.Null(jo["offset.Orders"]);
        }

        [Fact]
        public void SerializeRequest_WithMultiplePortals_ProducesAllDotNotationKeys()
        {
            var req = new FindRequest<User> { Layout = "layout" };
            req.AddQuery(new User { Id = 1 }, false);
            req.ConfigurePortal("Orders", limit: 50, offset: 1);
            req.ConfigurePortal("LineItems", limit: 200);

            var json = req.SerializeRequest();
            var jo = JObject.Parse(json);

            var portalNames = jo["portal"].ToObject<List<string>>();
            Assert.Contains("Orders", portalNames);
            Assert.Contains("LineItems", portalNames);
            Assert.Equal(50, jo["limit.Orders"].Value<int>());
            Assert.Equal(1, jo["offset.Orders"].Value<int>());
            Assert.Equal(200, jo["limit.LineItems"].Value<int>());
            Assert.Null(jo["offset.LineItems"]);
        }

        #endregion

        #region GET Endpoint Tests

        [Fact]
        public void GetRecordsEndpoint_WithPortals_IncludesPortalQueryParams()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var portals = new List<PortalRequestData>
            {
                new PortalRequestData { PortalName = "Orders", Limit = 100, Offset = 5 }
            };

            var url = fdc.GetRecordsEndpoint("layout", 10, 1, portals);

            Assert.Contains("portal=", url);
            Assert.Contains("_limit.Orders=100", url);
            Assert.Contains("_offset.Orders=5", url);
        }

        [Fact]
        public void GetRecordsEndpoint_WithNullPortals_ProducesBaseUrl()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var url = fdc.GetRecordsEndpoint("layout", 10, 1, null);

            Assert.DoesNotContain("portal=", url);
            Assert.Contains("_limit=10", url);
            Assert.Contains("_offset=1", url);
        }

        [Fact]
        public void GetRecordsEndpoint_WithEmptyPortals_ProducesBaseUrl()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var url = fdc.GetRecordsEndpoint("layout", 10, 1, new List<PortalRequestData>());

            Assert.DoesNotContain("portal=", url);
        }

        [Fact]
        public void GetRecordsEndpoint_WithMultiplePortals_IncludesAllQueryParams()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var portals = new List<PortalRequestData>
            {
                new PortalRequestData { PortalName = "Orders", Limit = 50 },
                new PortalRequestData { PortalName = "LineItems", Offset = 10 }
            };

            var url = fdc.GetRecordsEndpoint("layout", 10, 1, portals);

            Assert.Contains("_limit.Orders=50", url);
            Assert.DoesNotContain("_offset.Orders", url);
            Assert.DoesNotContain("_limit.LineItems", url);
            Assert.Contains("_offset.LineItems=10", url);
        }

        #endregion

        #region ConfigurePortal Tests

        [Fact]
        public void ConfigurePortal_DirectCall_ConfiguresPortal()
        {
            var req = new FindRequest<User> { Layout = "layout" };

            req.ConfigurePortal("Orders", limit: 100, offset: 5);

            Assert.Single(req.Portals);
            Assert.Equal("Orders", req.Portals.First().PortalName);
            Assert.Equal(100, req.Portals.First().Limit);
            Assert.Equal(5, req.Portals.First().Offset);
        }

        [Fact]
        public void ConfigurePortal_UpdatesExistingPortal()
        {
            var req = new FindRequest<User> { Layout = "layout" };

            req.ConfigurePortal("Orders", limit: 50);
            req.ConfigurePortal("Orders", offset: 10);

            Assert.Single(req.Portals);
            Assert.Equal(50, req.Portals.First().Limit);
            Assert.Equal(10, req.Portals.First().Offset);
        }

        #endregion

        #region Fluent Builder Tests

        [Fact]
        public void WithPortal_Limit_ProducesCorrectJson()
        {
            var req = new FindRequest<User> { Layout = "layout" };
            req.AddQuery(new User { Id = 1 }, false);

            req.WithPortal("Orders").Limit(100);

            var json = req.SerializeRequest();
            var jo = JObject.Parse(json);
            Assert.Equal(100, jo["limit.Orders"].Value<int>());
            Assert.Null(jo["offset.Orders"]);
        }

        [Fact]
        public void WithPortal_Offset_ProducesCorrectJson()
        {
            var req = new FindRequest<User> { Layout = "layout" };
            req.AddQuery(new User { Id = 1 }, false);

            req.WithPortal("Orders").Offset(5);

            var json = req.SerializeRequest();
            var jo = JObject.Parse(json);
            Assert.Null(jo["limit.Orders"]);
            Assert.Equal(5, jo["offset.Orders"].Value<int>());
        }

        [Fact]
        public void WithPortal_LimitAndOffset_ProducesCorrectJson()
        {
            var req = new FindRequest<User> { Layout = "layout" };
            req.AddQuery(new User { Id = 1 }, false);

            req.WithPortal("Orders").Limit(100).Offset(5);

            var json = req.SerializeRequest();
            var jo = JObject.Parse(json);
            Assert.Equal(100, jo["limit.Orders"].Value<int>());
            Assert.Equal(5, jo["offset.Orders"].Value<int>());
        }

        [Fact]
        public void WithPortal_MultiplePortals_ChainsCorrectly()
        {
            var req = new FindRequest<User> { Layout = "layout" };
            req.AddQuery(new User { Id = 1 }, false);

            req.WithPortal("Orders").Limit(100).Offset(5)
                .WithPortal("LineItems").Limit(200);

            Assert.Equal(2, req.Portals.Count);

            var json = req.SerializeRequest();
            var jo = JObject.Parse(json);
            Assert.Equal(100, jo["limit.Orders"].Value<int>());
            Assert.Equal(5, jo["offset.Orders"].Value<int>());
            Assert.Equal(200, jo["limit.LineItems"].Value<int>());
            Assert.Null(jo["offset.LineItems"]);
        }

        [Fact]
        public void WithPortal_NameOnly_IncludesPortalInArray()
        {
            var req = new FindRequest<User> { Layout = "layout" };
            req.AddQuery(new User { Id = 1 }, false);

            req.WithPortal("Orders");

            var json = req.SerializeRequest();
            var jo = JObject.Parse(json);
            Assert.Contains("Orders", jo["portal"].ToObject<List<string>>());
            Assert.Null(jo["limit.Orders"]);
            Assert.Null(jo["offset.Orders"]);
        }

        [Fact]
        public void IncludePortals_Extension_AddsMultiplePortals()
        {
            var req = new FindRequest<User> { Layout = "layout" };

            ((IFindRequest<User>)req).IncludePortals("Orders", "LineItems", "Contacts");

            Assert.Equal(3, req.Portals.Count);
        }

        #endregion

        #region SendAsync Round-Trip Tests

        [Fact]
        [System.Obsolete]
        public async Task SendAsync_FindWithPortalParams_SendsPortalKeysInRequestBody()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();
            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/{layout}/_find")
                .WithPartialContent("\"portal\"")
                .WithPartialContent("\"limit.action\"")
                .WithPartialContent("\"offset.action\"")
                .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFindWithPortal());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var req = new FindRequest<Dictionary<string, string>> { Layout = layout };
            req.AddQuery(new Dictionary<string, string> { { "ES_RECORD_ID", "111" } }, false);
            req.WithPortal("action").Limit(10).Offset(1);

            // act
            var response = await fdc.SendAsync(req);

            // assert — response came back, meaning our request body matched the mock expectations
            Assert.NotEmpty(response.Response.Data);
        }

        [Fact]
        [System.Obsolete]
        public async Task SendAsync_FindWithPortalParams_ReturnsPortalData()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();
            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/{layout}/_find")
                .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFindWithPortal());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var req = new FindRequest<Dictionary<string, string>> { Layout = layout };
            req.AddQuery(new Dictionary<string, string> { { "ES_RECORD_ID", "111" } }, false);
            req.ConfigurePortal("action", limit: 100, offset: 1);

            // act
            var response = await fdc.SendAsync(req);

            // assert — verify portal data is present in the response
            var firstRecord = response.Response.Data.First();
            Assert.NotNull(firstRecord.PortalData);
            Assert.True(firstRecord.PortalData.ContainsKey("action"));
            Assert.Equal(3, firstRecord.PortalData["action"].Count());
        }

        [Fact]
        [System.Obsolete]
        public async Task SendAsync_FindWithMultiplePortals_ReturnsAllPortalData()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();
            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/{layout}/_find")
                .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFindWithPortal());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var req = new FindRequest<Dictionary<string, string>> { Layout = layout };
            req.AddQuery(new Dictionary<string, string> { { "ES_RECORD_ID", "111" } }, false);
            req.WithPortal("action").Limit(50).Offset(1)
                .WithPortal("another-portal").Limit(100);

            // act
            var response = await fdc.SendAsync(req);

            // assert — both portals present in response
            var firstRecord = response.Response.Data.First();
            Assert.True(firstRecord.PortalData.ContainsKey("action"));
            Assert.True(firstRecord.PortalData.ContainsKey("another-portal"));
        }

        [Fact]
        public async Task SendAsync_EmptyQueryWithPortals_UsesGetEndpointAndReturnsData()
        {
            // arrange — empty query converts to GET request with portal query params
            var mockHttp = new MockHttpMessageHandler();
            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Get, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/{layout}/records*")
                .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var req = new FindRequest<User> { Layout = layout };
            // no AddQuery — this triggers the GET/records path
            req.WithPortal("RelatedItems").Limit(200).Offset(5);

            // act
            var response = await fdc.SendAsync(req);

            // assert — data returned via GET endpoint with portal params
            Assert.Equal(2, response.Count());
        }

        [Fact]
        public async Task SendAsync_FindWithPortals_WithoutPortalParams_ReturnsDataUnchanged()
        {
            // arrange — regression: no portals configured, behavior unchanged
            var mockHttp = new MockHttpMessageHandler();
            var layout = "the-layout";

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{FindTestsHelpers.Server}/fmi/data/v1/databases/{FindTestsHelpers.File}/layouts/{layout}/_find")
                .Respond(HttpStatusCode.OK, "application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), FindTestsHelpers.Connection);

            var req = new FindRequest<User> { Layout = layout };
            req.AddQuery(new User { Id = 1 }, false);
            // no portal configuration

            // act
            var response = await fdc.SendAsync(req);

            // assert
            Assert.Equal(2, response.Count());
        }

        #endregion
    }
}
