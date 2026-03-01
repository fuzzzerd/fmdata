using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FMData.Rest.Tests.TestModels;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Rest.Tests
{
    public class EditDeleteGetByIdTests
    {
        private static readonly string Server = "http://localhost";
        private static readonly string File = "test-file";
        private static readonly string Layout = "Users";

        private static ConnectionInfo TestConnection => new ConnectionInfo
        {
            FmsUri = Server,
            Database = File,
            Username = "unit",
            Password = "test"
        };

        private static MockHttpMessageHandler CreateMock()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Delete, $"{Server}/fmi/data/v1/databases/{File}/sessions*")
                .Respond(HttpStatusCode.OK, "application/json", "");

            return mockHttp;
        }

        #region EditAsync Overloads

        [Fact(DisplayName = "EditAsync(int, T) passthrough should succeed")]
        public async Task EditAsync_RecordIdAndModel_ShouldSucceed()
        {
            var mockHttp = CreateMock();

            mockHttp.When(new HttpMethod("PATCH"), $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records/42")
                .Respond("application/json", DataApiResponses.SuccessfulEdit());

            using var client = new FileMakerRestClient(mockHttp.ToHttpClient(), TestConnection);

            var response = await client.EditAsync(42, new User { Name = "updated" });

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        [Fact(DisplayName = "EditAsync(int, T, bool, bool) includes null values when flag set")]
        public async Task EditAsync_WithNullValueFlag_ShouldSucceed()
        {
            var mockHttp = CreateMock();

            mockHttp.When(new HttpMethod("PATCH"), $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records/42")
                .Respond("application/json", DataApiResponses.SuccessfulEdit());

            using var client = new FileMakerRestClient(mockHttp.ToHttpClient(), TestConnection);

            var response = await client.EditAsync(42, new User { Name = "updated", ForeignKeyId = null },
                includeNullValues: true, includeDefaultValues: true);

            Assert.NotNull(response);
            Assert.Contains(response.Messages, r => r.Message == "OK");
        }

        #endregion

        #region GetByFileMakerIdAsync Overloads

        [Fact(DisplayName = "GetByFileMakerIdAsync(int) infers layout from type")]
        public async Task GetByFileMakerIdAsync_InfersLayout()
        {
            var mockHttp = CreateMock();

            mockHttp.When(HttpMethod.Get, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records/4")
                .Respond("application/json", DataApiResponses.SuccessfulGetById(4));

            using var client = new FileMakerRestClient(mockHttp.ToHttpClient(), TestConnection);

            var result = await client.GetByFileMakerIdAsync<User>(4, fmId: null);

            Assert.NotNull(result);
            Assert.Equal("fuzzzerd", result.Name);
        }

        [Fact(DisplayName = "GetByFileMakerIdAsync with fmIdFunc maps RecordId")]
        public async Task GetByFileMakerIdAsync_WithFmIdFunc_MapsRecordId()
        {
            var mockHttp = CreateMock();

            mockHttp.When(HttpMethod.Get, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records/4")
                .Respond("application/json", DataApiResponses.SuccessfulGetById(4));

            using var client = new FileMakerRestClient(mockHttp.ToHttpClient(), TestConnection);

            var result = await client.GetByFileMakerIdAsync<User>(4,
                fmId: (o, id) => o.FileMakerRecordId = id);

            Assert.NotNull(result);
            Assert.Equal(4, result.FileMakerRecordId);
        }

        [Fact(DisplayName = "GetByFileMakerIdAsync with both mappers maps RecordId and ModId")]
        public async Task GetByFileMakerIdAsync_WithBothMappers_MapsBothIds()
        {
            var mockHttp = CreateMock();

            mockHttp.When(HttpMethod.Get, $"{Server}/fmi/data/v1/databases/{File}/layouts/{Layout}/records/4")
                .Respond("application/json", DataApiResponses.SuccessfulGetById(4));

            using var client = new FileMakerRestClient(mockHttp.ToHttpClient(), TestConnection);

            var result = await client.GetByFileMakerIdAsync<User>(4,
                fmId: (o, id) => o.FileMakerRecordId = id,
                fmMod: (o, modId) => o.FileMakerModId = modId);

            Assert.NotNull(result);
            Assert.Equal(4, result.FileMakerRecordId);
            Assert.Equal(0, result.FileMakerModId);
        }

        [Fact(DisplayName = "GetByFileMakerIdAsync with explicit layout works")]
        public async Task GetByFileMakerIdAsync_WithExplicitLayout()
        {
            var mockHttp = CreateMock();

            mockHttp.When(HttpMethod.Get, $"{Server}/fmi/data/v1/databases/{File}/layouts/CustomLayout/records/7")
                .Respond("application/json", DataApiResponses.SuccessfulGetById(7));

            using var client = new FileMakerRestClient(mockHttp.ToHttpClient(), TestConnection);

            var result = await client.GetByFileMakerIdAsync<User>("CustomLayout", 7,
                fmId: (o, id) => o.FileMakerRecordId = id);

            Assert.NotNull(result);
            Assert.Equal(7, result.FileMakerRecordId);
        }

        #endregion

        #region ProcessContainers (batch)

        [Fact(DisplayName = "ProcessContainers processes multiple models")]
        public async Task ProcessContainers_ProcessesMultipleModels()
        {
            var mockHttp = CreateMock();

            // container URLs need to be valid URIs
            var url1 = $"{Server}/container1.jpg";
            var url2 = $"{Server}/container2.jpg";

            mockHttp.When(HttpMethod.Get, url1)
                .Respond("application/octet-stream", new System.IO.MemoryStream(new byte[] { 1, 2, 3 }));

            mockHttp.When(HttpMethod.Get, url2)
                .Respond("application/octet-stream", new System.IO.MemoryStream(new byte[] { 4, 5, 6 }));

            using var client = new FileMakerRestClient(mockHttp.ToHttpClient(), TestConnection);

            var models = new[]
            {
                new ContainerFieldTestModel { SomeContainerField = url1 },
                new ContainerFieldTestModel { SomeContainerField = url2 }
            };

            await client.ProcessContainers(models);

            Assert.NotNull(models[0].SomeContainerFieldData);
            Assert.NotNull(models[1].SomeContainerFieldData);
        }

        #endregion

        #region Obsolete Metadata Methods with Database Validation

        [Fact(DisplayName = "GetLayoutsAsync(database) throws when database doesn't match")]
        public async Task GetLayoutsAsync_WithWrongDatabase_Throws()
        {
            var mockHttp = CreateMock();
            using var client = new FileMakerRestClient(mockHttp.ToHttpClient(), TestConnection);

#pragma warning disable CS0618 // intentionally testing the obsolete overload
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => client.GetLayoutsAsync("wrong-database"));
#pragma warning restore CS0618
        }

        [Fact(DisplayName = "GetScriptsAsync(database) throws when database doesn't match")]
        public async Task GetScriptsAsync_WithWrongDatabase_Throws()
        {
            var mockHttp = CreateMock();
            using var client = new FileMakerRestClient(mockHttp.ToHttpClient(), TestConnection);

#pragma warning disable CS0618 // intentionally testing the obsolete overload
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => client.GetScriptsAsync("wrong-database"));
#pragma warning restore CS0618
        }

        [Fact(DisplayName = "GetLayoutAsync(database, layout) throws when database doesn't match")]
        public async Task GetLayoutAsync_WithWrongDatabase_Throws()
        {
            var mockHttp = CreateMock();
            using var client = new FileMakerRestClient(mockHttp.ToHttpClient(), TestConnection);

#pragma warning disable CS0618 // intentionally testing the obsolete overload
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => client.GetLayoutAsync("wrong-database", "layout"));
#pragma warning restore CS0618
        }

        #endregion
    }
}
