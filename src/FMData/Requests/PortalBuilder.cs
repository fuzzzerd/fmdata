namespace FMData
{
    /// <summary>
    /// Fluent builder for configuring per-portal limit and offset parameters.
    /// Obtained by calling <see cref="FindRequestExtensions.WithPortal{T}"/> on a find request.
    /// </summary>
    /// <typeparam name="T">The type used for the find request/response.</typeparam>
    public class PortalBuilder<T>
    {
        private readonly IFindRequest<T> _request;
        private readonly string _portalName;

        internal PortalBuilder(IFindRequest<T> request, string portalName)
        {
            _request = request;
            _portalName = portalName;
            _request.ConfigurePortal(portalName);
        }

        /// <summary>
        /// Set the maximum number of portal records to return.
        /// </summary>
        /// <param name="limit">The maximum number of portal records.</param>
        /// <returns>This builder instance for chaining.</returns>
        public PortalBuilder<T> Limit(int limit)
        {
            _request.ConfigurePortal(_portalName, limit: limit);
            return this;
        }

        /// <summary>
        /// Set the number of portal records to skip before returning results.
        /// </summary>
        /// <param name="offset">The number of portal records to skip.</param>
        /// <returns>This builder instance for chaining.</returns>
        public PortalBuilder<T> Offset(int offset)
        {
            _request.ConfigurePortal(_portalName, offset: offset);
            return this;
        }

        /// <summary>
        /// Start configuring another portal on the same request.
        /// </summary>
        /// <param name="portalName">The name of the portal (table occurrence).</param>
        /// <returns>A new builder instance for the specified portal.</returns>
        public PortalBuilder<T> WithPortal(string portalName)
        {
            return new PortalBuilder<T>(_request, portalName);
        }
    }
}
