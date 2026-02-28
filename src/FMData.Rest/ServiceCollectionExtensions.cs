#if NETSTANDARD2_0_OR_GREATER || NET6_0_OR_GREATER
using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FMData.Rest
{
    /// <summary>
    /// Extension methods for registering FileMakerRestClient with dependency injection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a <see cref="FileMakerRestClient"/> to the service collection using <see cref="IHttpClientFactory"/>.
        /// Registers the client as both <see cref="IFileMakerApiClient"/> and <see cref="IFileMakerRestClient"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureConnection">Action to configure the connection info.</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for further HTTP client configuration.</returns>
        public static IHttpClientBuilder AddFMDataRest(
            this IServiceCollection services,
            Action<ConnectionInfo> configureConnection)
        {
            return AddFMDataRest(services, configureConnection, null);
        }

        /// <summary>
        /// Adds a <see cref="FileMakerRestClient"/> to the service collection using <see cref="IHttpClientFactory"/>.
        /// Registers the client as both <see cref="IFileMakerApiClient"/> and <see cref="IFileMakerRestClient"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureConnection">Action to configure the connection info.</param>
        /// <param name="configureHttpClient">Optional action to configure the HttpClient (e.g., set timeouts or default headers).</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for further HTTP client configuration.</returns>
        public static IHttpClientBuilder AddFMDataRest(
            this IServiceCollection services,
            Action<ConnectionInfo> configureConnection,
            Action<HttpClient> configureHttpClient)
        {
            var conn = new ConnectionInfo();
            configureConnection(conn);

            services.AddSingleton(conn);
            services.AddSingleton<IAuthTokenProvider>(new DefaultAuthTokenProvider(conn));

            // Register the main FMData named client
            var builder = services.AddHttpClient("FMData", client =>
            {
                configureHttpClient?.Invoke(client);
            });

            // Register FileMakerRestClient as singleton (preserves auth token state across requests)
            services.AddSingleton<FileMakerRestClient>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                var authProvider = sp.GetRequiredService<IAuthTokenProvider>();
                return new FileMakerRestClient(factory, authProvider);
            });

            services.AddSingleton<IFileMakerApiClient>(sp => sp.GetRequiredService<FileMakerRestClient>());
            services.AddSingleton<IFileMakerRestClient>(sp => sp.GetRequiredService<FileMakerRestClient>());

            return builder;
        }
    }
}
#endif
