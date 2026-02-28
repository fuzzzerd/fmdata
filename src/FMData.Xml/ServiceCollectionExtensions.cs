#if NETSTANDARD2_0_OR_GREATER || NET6_0_OR_GREATER
using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FMData.Xml
{
    /// <summary>
    /// Extension methods for registering FileMakerXmlClient with dependency injection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a <see cref="FileMakerXmlClient"/> to the service collection using <see cref="IHttpClientFactory"/>.
        /// Registers the client as <see cref="IFileMakerApiClient"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureConnection">Action to configure the connection info.</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for further HTTP client configuration.</returns>
        public static IHttpClientBuilder AddFMDataXml(
            this IServiceCollection services,
            Action<ConnectionInfo> configureConnection)
        {
            return AddFMDataXml(services, configureConnection, null);
        }

        /// <summary>
        /// Adds a <see cref="FileMakerXmlClient"/> to the service collection using <see cref="IHttpClientFactory"/>.
        /// Registers the client as <see cref="IFileMakerApiClient"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureConnection">Action to configure the connection info.</param>
        /// <param name="configureHttpClient">Optional action to configure the HttpClient (e.g., set timeouts or default headers).</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for further HTTP client configuration.</returns>
        public static IHttpClientBuilder AddFMDataXml(
            this IServiceCollection services,
            Action<ConnectionInfo> configureConnection,
            Action<HttpClient> configureHttpClient)
        {
            var conn = new ConnectionInfo();
            configureConnection(conn);

            services.AddSingleton(conn);

            // Register the named client with credentials on the handler
            var builder = services.AddHttpClient("FMData.Xml", client =>
            {
                configureHttpClient?.Invoke(client);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                Credentials = new NetworkCredential(conn.Username, conn.Password)
            });

            // Register FileMakerXmlClient as singleton
            services.AddSingleton<IFileMakerApiClient>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                return new FileMakerXmlClient(factory, conn);
            });

            return builder;
        }
    }
}
#endif
