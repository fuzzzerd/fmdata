using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FMData.Rest.Tests.TestModels;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Rest.Tests
{
    public class HttpClientFactoryTests
    {
        private static readonly string Server = "http://localhost";
        private static readonly string File = "test-file";
        private static readonly string User = "unit";
        private static readonly string Pass = "test";

        private static ConnectionInfo TestConnectionInfo => new ConnectionInfo
        {
            FmsUri = Server,
            Database = File,
            Username = User,
            Password = Pass
        };

        /// <summary>
        /// IHttpClientFactory implementation for testing that delegates to a MockHttpMessageHandler
        /// and tracks all CreateClient calls.
        /// </summary>
        private class TestHttpClientFactory : IHttpClientFactory
        {
            private readonly MockHttpMessageHandler _mockHandler;
            public int CreateClientCallCount { get; private set; }
            public string LastClientName { get; private set; }
            public List<string> AllRequestedNames { get; } = new List<string>();

            public TestHttpClientFactory(MockHttpMessageHandler mockHandler)
            {
                _mockHandler = mockHandler;
            }

            public HttpClient CreateClient(string name)
            {
                CreateClientCallCount++;
                LastClientName = name;
                AllRequestedNames.Add(name);
                return _mockHandler.ToHttpClient();
            }
        }

        /// <summary>
        /// Creates a MockHttpMessageHandler with standard auth and find endpoints configured.
        /// </summary>
        private static MockHttpMessageHandler CreateMockWithAuthAndFind(string layout = "Users")
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Delete, $"{Server}/fmi/data/v1/databases/{File}/sessions*")
                .Respond(HttpStatusCode.OK, "application/json", "");

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/{layout}/_find")
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            return mockHttp;
        }

        [Fact(DisplayName = "Can construct FileMakerRestClient with IHttpClientFactory and ConnectionInfo")]
        public void CanConstruct_WithFactory_AndConnectionInfo()
        {
            var mockHttp = new MockHttpMessageHandler();
            var factory = new TestHttpClientFactory(mockHttp);

            using var client = new FileMakerRestClient(factory, TestConnectionInfo);

            Assert.NotNull(client);
            Assert.False(client.IsAuthenticated);
        }

        [Fact(DisplayName = "Can construct FileMakerRestClient with IHttpClientFactory and IAuthTokenProvider")]
        public void CanConstruct_WithFactory_AndAuthTokenProvider()
        {
            var mockHttp = new MockHttpMessageHandler();
            var factory = new TestHttpClientFactory(mockHttp);
            var authProvider = new DefaultAuthTokenProvider(TestConnectionInfo);

            using var client = new FileMakerRestClient(factory, authProvider);

            Assert.NotNull(client);
            Assert.False(client.IsAuthenticated);
        }

        [Fact(DisplayName = "Factory constructor calls CreateClient to obtain HttpClient")]
        public void FactoryConstructor_CallsCreateClient()
        {
            var mockHttp = new MockHttpMessageHandler();
            var factory = new TestHttpClientFactory(mockHttp);

            using var client = new FileMakerRestClient(factory, TestConnectionInfo);

            Assert.Equal(1, factory.CreateClientCallCount);
        }

        [Fact(DisplayName = "FileMakerRestClient from factory produces correct auth endpoint")]
        public void FactoryClient_HasCorrectEndpoint()
        {
            var mockHttp = new MockHttpMessageHandler();
            var factory = new TestHttpClientFactory(mockHttp);

            using var client = new FileMakerRestClient(factory, TestConnectionInfo);

            Assert.Equal("http://localhost/fmi/data/v1/databases/test-file/sessions", client.AuthEndpoint());
        }

        [Fact(DisplayName = "AddFMDataRest registers IFileMakerApiClient")]
        public void AddFMDataRest_Registers_IFileMakerApiClient()
        {
            var services = new ServiceCollection();

            services.AddFMDataRest(conn =>
            {
                conn.FmsUri = "http://localhost";
                conn.Database = "test-file";
                conn.Username = "unit";
                conn.Password = "test";
            });

            var provider = services.BuildServiceProvider();
            var client = provider.GetService<IFileMakerApiClient>();

            Assert.NotNull(client);
            Assert.IsType<FileMakerRestClient>(client);
        }

        [Fact(DisplayName = "AddFMDataRest registers IFileMakerRestClient")]
        public void AddFMDataRest_Registers_IFileMakerRestClient()
        {
            var services = new ServiceCollection();

            services.AddFMDataRest(conn =>
            {
                conn.FmsUri = "http://localhost";
                conn.Database = "test-file";
                conn.Username = "unit";
                conn.Password = "test";
            });

            var provider = services.BuildServiceProvider();
            var client = provider.GetService<IFileMakerRestClient>();

            Assert.NotNull(client);
            Assert.IsType<FileMakerRestClient>(client);
        }

        [Fact(DisplayName = "AddFMDataRest resolves same singleton instance for both interfaces")]
        public void AddFMDataRest_ResolvesSameSingleton()
        {
            var services = new ServiceCollection();

            services.AddFMDataRest(conn =>
            {
                conn.FmsUri = "http://localhost";
                conn.Database = "test-file";
                conn.Username = "unit";
                conn.Password = "test";
            });

            var provider = services.BuildServiceProvider();
            var apiClient = provider.GetService<IFileMakerApiClient>();
            var restClient = provider.GetService<IFileMakerRestClient>();

            Assert.Same(apiClient, restClient);
        }

        [Fact(DisplayName = "AddFMDataRest registers ConnectionInfo")]
        public void AddFMDataRest_RegistersConnectionInfo()
        {
            var services = new ServiceCollection();

            services.AddFMDataRest(conn =>
            {
                conn.FmsUri = "http://localhost";
                conn.Database = "my-db";
                conn.Username = "admin";
                conn.Password = "secret";
            });

            var provider = services.BuildServiceProvider();
            var conn = provider.GetService<ConnectionInfo>();

            Assert.NotNull(conn);
            Assert.Equal("http://localhost", conn.FmsUri);
            Assert.Equal("my-db", conn.Database);
        }

        [Fact(DisplayName = "AddFMDataRest with configureHttpClient callback is invoked")]
        public void AddFMDataRest_ConfigureHttpClient_IsInvoked()
        {
            var services = new ServiceCollection();
            var callbackInvoked = false;

            services.AddFMDataRest(
                conn =>
                {
                    conn.FmsUri = "http://localhost";
                    conn.Database = "test-file";
                    conn.Username = "unit";
                    conn.Password = "test";
                },
                client =>
                {
                    callbackInvoked = true;
                    client.Timeout = System.TimeSpan.FromSeconds(30);
                });

            var provider = services.BuildServiceProvider();
            // Resolve to trigger the factory
            var httpFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpFactory.CreateClient("FMData");

            Assert.True(callbackInvoked);
        }

        #region Behavioral Tests

        [Fact(DisplayName = "Factory-created client can authenticate")]
        public async Task FactoryClient_CanAuthenticate()
        {
            // arrange
            var mockHttp = CreateMockWithAuthAndFind();
            var factory = new TestHttpClientFactory(mockHttp);

            using var client = new FileMakerRestClient(factory, TestConnectionInfo);

            // act
            var response = await client.RefreshTokenAsync();

            // assert
            Assert.True(client.IsAuthenticated);
            Assert.Contains(response.Messages, m => m.Code == "0");
        }

        [Fact(DisplayName = "Factory-created client can execute FindAsync")]
        public async Task FactoryClient_CanExecuteFindAsync()
        {
            // arrange
            var mockHttp = CreateMockWithAuthAndFind();
            var factory = new TestHttpClientFactory(mockHttp);

            using var client = new FileMakerRestClient(factory, TestConnectionInfo);

            // act
            var results = await client.FindAsync(new User { Name = "fuzzzerd" });

            // assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.Name == "fuzzzerd");
        }

        [Fact(DisplayName = "Factory-created client can execute CreateAsync")]
        public async Task FactoryClient_CanExecuteCreateAsync()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Delete, $"{Server}/fmi/data/v1/databases/{File}/sessions*")
                .Respond(HttpStatusCode.OK, "application/json", "");

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/layouts/Users/records*")
                .Respond("application/json", DataApiResponses.SuccessfulCreate());

            var factory = new TestHttpClientFactory(mockHttp);
            using var client = new FileMakerRestClient(factory, TestConnectionInfo);

            // act
            var response = await client.CreateAsync(new User { Name = "test" });

            // assert
            Assert.NotNull(response);
            Assert.Contains(response.Messages, m => m.Message == "OK");
        }

        [Fact(DisplayName = "Container download uses factory when constructed with IHttpClientFactory")]
        public async Task ContainerDownload_UsesFactory()
        {
            // arrange
            var containerUrl = $"{Server}/fmi/data/v1/databases/{File}/layouts/Users/records/1/containers/SomeContainerField/1";
            var containerBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG magic bytes

            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Delete, $"{Server}/fmi/data/v1/databases/{File}/sessions*")
                .Respond(HttpStatusCode.OK, "application/json", "");

            mockHttp.When(HttpMethod.Get, containerUrl)
                .Respond("application/octet-stream", new System.IO.MemoryStream(containerBytes));

            var factory = new TestHttpClientFactory(mockHttp);
            using var client = new FileMakerRestClient(factory, TestConnectionInfo);

            // act — ProcessContainer will call GetContainerOnClient internally
            var model = new ContainerFieldTestModel
            {
                SomeContainerField = containerUrl
            };
            await client.ProcessContainer(model);

            // assert — factory should have been called more than just the initial construction call
            // (once for construction + once for the container download)
            Assert.True(factory.CreateClientCallCount > 1,
                $"Expected factory to be called for container download, but CreateClient was only called {factory.CreateClientCallCount} time(s)");
        }

        [Theory(DisplayName = "Factory-created client respects RestTargetVersion")]
        [InlineData(RestTargetVersion.v1, "v1")]
        [InlineData(RestTargetVersion.v2, "v2")]
        [InlineData(RestTargetVersion.vLatest, "vLatest")]
        public void FactoryClient_RespectsTargetVersion(RestTargetVersion version, string expectedSegment)
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();
            var factory = new TestHttpClientFactory(mockHttp);
            var conn = new ConnectionInfo
            {
                FmsUri = Server,
                Database = File,
                Username = User,
                Password = Pass,
                RestTargetVersion = version
            };

            // act
            using var client = new FileMakerRestClient(factory, conn);

            // assert
            Assert.Contains($"/fmi/data/{expectedSegment}/", client.AuthEndpoint());
        }

        [Fact(DisplayName = "Factory-created client sets User-Agent header")]
        public async Task FactoryClient_SetsUserAgent()
        {
            // arrange
            var fmrAssembly = System.Reflection.Assembly.GetExecutingAssembly()
                .GetReferencedAssemblies()
                .Single(a => a.Name.StartsWith("FMData.Rest"));
            var asm = System.Reflection.Assembly.Load(fmrAssembly.ToString());
            var fmrVer = System.Diagnostics.FileVersionInfo.GetVersionInfo(asm.Location).ProductVersion;

            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{Server}/fmi/data/v1/databases/{File}/sessions")
                .WithHeaders("User-Agent", $"{fmrAssembly.Name}/{fmrVer}")
                .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Delete, $"{Server}/fmi/data/v1/databases/{File}/sessions*")
                .Respond(HttpStatusCode.OK, "application/json", "");

            var factory = new TestHttpClientFactory(mockHttp);
            using var client = new FileMakerRestClient(factory, TestConnectionInfo);

            // act — will fail if User-Agent header doesn't match
            await client.RefreshTokenAsync();

            // assert
            Assert.True(client.IsAuthenticated);
        }

        #endregion
    }
}
