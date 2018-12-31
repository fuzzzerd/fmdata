namespace FMData
{
    /// <summary>
    /// Extension (utility and helper) methods for IFileMakerRequest.
    /// </summary>
    public static class RequestExtensions
    {
        /// <summary>
        /// Adds a pre request script to the request, with a parameter.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="scriptName">Name of the script to be called.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static IFileMakerRequest SetPreRequestScript(
            this IFileMakerRequest request, 
            string scriptName, 
            string scriptParameter = null)
        {
            request.PreRequestScript = scriptName;
            if (!string.IsNullOrEmpty(scriptParameter)) request.PreRequestScriptParameter = scriptParameter;
            return request;
        }

        /// <summary>
        /// Adds a pre sort script to the request, with a parameter.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="scriptName">Name of the script to be called.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static IFileMakerRequest SetPreSortScript(
            this IFileMakerRequest request, 
            string scriptName, 
            string scriptParameter = null)
        {
            request.PreSortScript = scriptName;
            if (!string.IsNullOrEmpty(scriptParameter)) request.PreSortScriptParameter = scriptParameter;
            return request;
        }

        /// <summary>
        /// Adds a script to the request, with a parameter.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="scriptName">Name of the script to be called.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static IFileMakerRequest SetScript(
            this IFileMakerRequest request, 
            string scriptName, 
            string scriptParameter = null)
        {
            request.Script = scriptName;
            if (!string.IsNullOrEmpty(scriptParameter)) request.ScriptParameter = scriptParameter;
            return request;
        }
    }
}