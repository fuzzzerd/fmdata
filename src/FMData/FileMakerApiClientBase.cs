using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace FMData
{
    /// <summary>
    /// Base for implementations to inherit from.
    /// Provides some wrapper and passthrough functionality to expand the API surface without requiring each fo those methods be re-implemented.
    /// </summary>
    public abstract partial class FileMakerApiClientBase : IFileMakerApiClient, IDisposable
    {
        #region Request Factory Abstracts
        /// <summary>
        /// Make a new instance of the Create Request class for Type T.
        /// </summary>
        public ICreateRequest<T> GenerateCreateRequest<T>() => _createFactory<T>();

        /// <summary>
        /// Generates a new create request for the input data.
        /// </summary>
        /// <param name="data">The initial find request data.</param>
        /// <typeparam name="T">The type used for the create request.</typeparam>
        /// <returns>An IFindRequest{T} instance setup per the initial query paramater.</returns>
        public ICreateRequest<T> GenerateCreateRequest<T>(T data)
        {
            return GenerateCreateRequest<T>().SetData(data).UseLayout(data);
        }

        /// <summary>
        /// Factory to get a new Create Request of the correct type.
        /// </summary>
        protected abstract ICreateRequest<T> _createFactory<T>();

        /// <summary>
        /// Generates a new edit request for the input object.
        /// </summary>
        /// <param name="data">The initial edit data request.</param>
        /// <typeparam name="T">The type used for the edit request.</typeparam>
        /// <returns>An IEditRequest{T} instance setup per the initial query paramater.</returns>
        public IEditRequest<T> GenerateEditRequest<T>(T data)
        {
            return GenerateEditRequest<T>().SetData(data).UseLayout(data);
        }

        /// <summary>
        /// Make a new instance of the Edit Request class for Type T.
        /// </summary>
        public IEditRequest<T> GenerateEditRequest<T>() => _editFactory<T>();

        /// <summary>
        /// Factory to get a new Edit Request of the correct type.
        /// </summary>
        protected abstract IEditRequest<T> _editFactory<T>();

        /// <summary>
        /// Generates a new find request with an initial find query instance, specifying the layout via the model's DataContract attribute.
        /// </summary>
        /// <param name="initialQuery">The initial find request data.</param>
        /// <typeparam name="T">The type used for the find request.</typeparam>
        /// <returns>An IFindRequest{T} instance setup per the initial query paramater.</returns>
        public IFindRequest<T> GenerateFindRequest<T>(T initialQuery)
        {
            return GenerateFindRequest<T>().AddCriteria(initialQuery, false).UseLayout(initialQuery);
        }

        /// <summary>
        /// Make a new instance of the Find Request for Type T.
        /// </summary>
        public IFindRequest<T> GenerateFindRequest<T>() => _findFactory<T>();

        /// <summary>
        /// Factory to get a new Find Request of the correct type.
        /// </summary>
        protected abstract IFindRequest<T> _findFactory<T>();

        /// <summary>
        /// Make a new instance of the Delete Request.
        /// </summary>
        public IDeleteRequest GenerateDeleteRequest() => _deleteFactory();
        /// <summary>
        /// Factory to get a new Delete Request of the correct type.
        /// </summary>
        protected abstract IDeleteRequest _deleteFactory();
        #endregion

        /// <summary>
        /// HttpClient for connecting to FMS. Injected or newed up for each instance of the client.
        /// </summary>
        protected readonly HttpClient _client;
        /// <summary>
        /// Uri to FileMaker Server
        /// </summary>
        protected readonly string _fmsUri;
        /// <summary>
        /// Database/FileMaker File we're connected/ing to.
        /// </summary>
        protected readonly string _fileName;
        /// <summary>
        /// Username for connections.
        /// </summary>
        protected readonly string _userName;
        /// <summary>
        /// Password for connections.
        /// </summary>
        protected readonly string _password;

        #region Constructors
        /// <summary>
        /// FM Data Constructor with HttpClient and ConnectionInfo. Useful for Dependency Injection situations
        /// </summary>
        /// <param name="client">The HttpClient instance to use.</param>
        /// <param name="conn">The connection information for FMS.</param>
        public FileMakerApiClientBase(HttpClient client, ConnectionInfo conn)
        {
            _client = client;

            _fmsUri = conn.FmsUri;
            // trim out the trailing slash if they included it
            if (_fmsUri.EndsWith("/", StringComparison.CurrentCultureIgnoreCase))
            {
                _fmsUri = conn.FmsUri.Substring(0, conn.FmsUri.Length - 1);
            }
            _fileName = Uri.EscapeDataString(conn.Database);

            _userName = conn.Username;
            _password = conn.Password;
        }
        #endregion

        #region Create
        /// <summary>
        /// Create a record in the database utilizing the DataContract to target the layout.
        /// </summary>
        /// <typeparam name="T">The type parameter to be created.</typeparam>
        /// <param name="input">Object containing the data to be on the newly created record.</param>
        /// <returns></returns>
        public Task<ICreateResponse> CreateAsync<T>(T input) where T : class, new() => CreateAsync(input, false, false); // maintain backward compatibility.

        /// <summary>
        /// Create a record in the database utilizing the DataContract attribute to target the layout.
        /// </summary>
        /// <typeparam name="T">The type parameter to be created.</typeparam>
        /// <param name="input">Object containing the data to be on the newly created record.</param>
        /// <param name="includeNullValues">Dictates the serialization behavior regarding null values.</param>
        /// <param name="includeDefaultValues">Dictates the serialization behavior regarding default values.</param>
        public Task<ICreateResponse> CreateAsync<T>(T input, bool includeNullValues, bool includeDefaultValues) where T : class, new()
        {
            var request = GenerateCreateRequest(input);
            request.IncludeNullValuesInSerializedOutput = includeNullValues;
            request.IncludeDefaultValuesInSerializedOutput = includeDefaultValues;
            return SendAsync(request);
        }

        /// <summary>
        /// Create a record in the file via explicit layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="layout">The layout to use for the context of the request.</param>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns>The newly created RecordId and/or an error response code.</returns>
        public Task<ICreateResponse> CreateAsync<T>(string layout, T input) where T : class, new()
        {
            var request = GenerateCreateRequest(input);
            request.Layout = layout;
            return SendAsync(request);
        }

        /// <summary>
        /// Create a record in the file, attempt to use the [TableAttribute] to determine the layout and perform a script with parameter.
        /// </summary>
        /// <typeparam name="T">The type to create</typeparam>
        /// <param name="input">The input record to create.</param>
        /// <param name="script">The name of a FileMaker script to run.</param>
        /// <param name="scriptParameter">The parameter to pass to the script.</param>
        /// <returns></returns>
        public Task<ICreateResponse> CreateAsync<T>(
            T input,
            string script,
            string scriptParameter) where T : class, new()
        {
            var request = GenerateCreateRequest(input);
            request.Script = script;
            request.ScriptParameter = scriptParameter;
            return SendAsync(request);
        }

        /// <summary>
        /// Creates a record matching the input data. All possible scripts available.
        /// Empty script names will be ignored.
        /// </summary>
        /// <typeparam name="T">The type of record to be created.</typeparam>
        /// <param name="input">The data to put in the record.</param>
        /// <param name="script">Name of the script to run at request completion.</param>
        /// <param name="scriptParameter">Parameter for script.</param>
        /// <param name="preRequestScript">Script to run before the request. See FMS documentation for more details.</param>
        /// <param name="preRequestScriptParameter">Parameter for script.</param>
        /// <param name="preSortScript">Script to run after the request, but before the sort. See FMS documentation for more details.</param>
        /// <param name="preSortScriptParameter">Parameter for script.</param>
        /// <returns>A response indicating the results of the call to the FileMaker Server Data API.</returns>
        public Task<ICreateResponse> CreateAsync<T>(
            T input,
            string script,
            string scriptParameter,
            string preRequestScript,
            string preRequestScriptParameter,
            string preSortScript,
            string preSortScriptParameter) where T : class, new()
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var request = GenerateCreateRequest(input);

            if (!string.IsNullOrEmpty(script))
            {
                request.Script = script;
                request.ScriptParameter = scriptParameter;
            }
            if (!string.IsNullOrEmpty(preRequestScript))
            {
                request.PreRequestScript = preRequestScript;
                request.PreRequestScriptParameter = preRequestScriptParameter;
            }

            if (!string.IsNullOrEmpty(preSortScript))
            {
                request.PreSortScript = preSortScript;
                request.PreSortScriptParameter = preSortScriptParameter;
            }

            return SendAsync(request);
        }
        #endregion

        #region Get Metadata

        /// <summary>
        /// Get FileMaker Server Product Information.
        /// </summary>
        /// <returns>An instance of the FileMaker Product Info.</returns>
        public abstract Task<ProductInformation> GetProductInformationAsync();
        #endregion

        #region Find
        /// <summary>
        /// Find a record with utilizing a class instance to define the find request field values.
        /// </summary>
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="request">The object with properties to map to the find request.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public Task<IEnumerable<T>> FindAsync<T>(
            T request) where T : class, new()
        {
            var req = GenerateFindRequest(request);
            return SendAsync(req);
        }

        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <param name="skip">Number of records to skip.</param>
        /// <param name="take">Number of records to return.</param>
        /// <returns></returns>
        public Task<IEnumerable<T>> FindAsync<T>(
            T request,
            int skip,
            int take) where T : class, new()
        {
            var req = this.GenerateFindRequest(request).SetLimit(take).SetOffset(skip);
            return SendAsync(req);
        }

        /// <summary>
        /// Find a record with utilizing a class instance to define the find request field values.
        /// </summary>
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="request">The object with properties to map to the find request.</param>
        /// <param name="fmIdFunc">Function to map a the FileMaker RecordId to each instance T.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public Task<IEnumerable<T>> FindAsync<T>(
            T request,
            Func<T, int, object> fmIdFunc) where T : class, new()
        {
            var req = GenerateFindRequest(request);
            return SendAsync(req, fmIdFunc);
        }

        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <param name="fmIdFunc">Function to map the FileMaker RecordId to each instance T.</param>
        /// <param name="skip">Number of records to skip.</param>
        /// <param name="take">Number of records to return.</param>
        /// <returns></returns>
        public Task<IEnumerable<T>> FindAsync<T>(
            T request,
            int skip,
            int take,
            Func<T, int, object> fmIdFunc) where T : class, new()
        {
            var req = GenerateFindRequest(request).SetLimit(take).SetOffset(skip);
            return SendAsync(req, fmIdFunc);
        }

        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <param name="script">Script to run after the request is completed.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <param name="fmIdFunc">Function to map the FileMaker RecordId to each instance T.</param>
        /// <returns></returns>
        public Task<IEnumerable<T>> FindAsync<T>(
             T request,
             string script,
             string scriptParameter,
             Func<T, int, object> fmIdFunc) where T : class, new()
        {
            var req = GenerateFindRequest(request)
                .SetLimit(100)
                .SetOffset(0);
            req.Script = script;
            req.ScriptParameter = scriptParameter;
            return SendAsync(req, fmIdFunc);
        }

        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <param name="skip">Number of records to skip.</param>
        /// <param name="take">Number of records to return.</param>
        /// <param name="script">Script to run after the request is completed.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <param name="fmIdFunc">Function to map the FileMaker RecordId to each instance T.</param>
        /// <returns></returns>
        public Task<IEnumerable<T>> FindAsync<T>(
            T request,
            int skip,
            int take,
            string script,
            string scriptParameter,
            Func<T, int, object> fmIdFunc) where T : class, new()
        {
            var req = GenerateFindRequest(request)
                .SetLimit(take)
                .SetOffset(skip);
            req.Script = script;
            req.ScriptParameter = scriptParameter;
            return SendAsync(req, fmIdFunc);
        }

        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <param name="skip">Number of records to skip.</param>
        /// <param name="take">Number of records to return.</param>
        /// <param name="script">Script to run after the request is completed.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <param name="fmIdFunc">Function to map the FileMaker RecordId to each instance T.</param>
        /// <param name="fmModIdFunc">Function to map hte FileMaker ModId to each instance of T.</param>
        /// <returns></returns>
        public Task<IEnumerable<T>> FindAsync<T>(
            T request,
            int skip,
            int take,
            string script,
            string scriptParameter,
            Func<T, int, object> fmIdFunc,
            Func<T, int, object> fmModIdFunc) where T : class, new()
        {
            var req = GenerateFindRequest(request)
                .SetLimit(take)
                .SetOffset(skip);
            req.Script = script;
            req.ScriptParameter = scriptParameter;
            return SendAsync(req, fmIdFunc, fmModIdFunc);
        }

        /// <summary>
        /// Strongly typed find request.
        /// </summary>
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="layout">The name of the layout to run this request on.</param>
        /// <param name="request">The object with properties to map to the find request.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public Task<IEnumerable<T>> FindAsync<T>(
            string layout,
            T request) where T : class, new()
        {
            var req = this.GenerateFindRequest<T>();
            req.Layout = layout;
            req.AddQuery(request, false);
            return SendAsync(req);
        }
        #endregion

        #region Edit
        /// <summary>
        /// Edit a record by FileMaker RecordId.
        /// </summary>
        /// <typeparam name="T">The type to pull the [Table] attribute from for context layout.</typeparam>
        /// <param name="recordId">The FileMaker RecordId of the record to be edited.</param>
        /// <param name="input">Object containing the values the record should reflect after the edit.</param>
        public Task<IEditResponse> EditAsync<T>(int recordId, T input) where T : class, new() => EditAsync(recordId, input, false, false); // maintain backward compatibility.

        /// <summary>
        /// Edit a record in the file, attempt to use the [TableAttribute] to determine the layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="recordId">The internal FileMaker RecordId of the record to edit.</param>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <param name="includeNullValues">Dictates the serialization behavior regarding null values.</param>
        /// <param name="includeDefaultValues">Dictates the serialization behavior regarding default values.</param>
        public Task<IEditResponse> EditAsync<T>(int recordId, T input, bool includeNullValues, bool includeDefaultValues) where T : class, new()
        {
            var request = GenerateEditRequest(input);
            request.RecordId = recordId;
            request.IncludeDefaultValuesInSerializedOutput = includeDefaultValues;
            request.IncludeNullValuesInSerializedOutput = includeNullValues;
            return SendAsync(request);
        }

        /// <summary>
        /// Edit a record in the file, attempt to use the [TableAttribute] to determine the layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="recordId">The internal FileMaker RecordId of the record to edit.</param>
        /// <param name="script">script to run after the request.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns></returns>
        public Task<IEditResponse> EditAsync<T>(
            int recordId,
            string script,
            string scriptParameter,
            T input) where T : class, new()
        {
            var request = GenerateEditRequest(input);
            request.RecordId = recordId;

            if (!string.IsNullOrEmpty(script))
            {
                request.Script = script;
                request.ScriptParameter = scriptParameter;
            }

            return SendAsync(request);
        }

        /// <summary>
        /// Edit a record.
        /// </summary>
        /// <typeparam name="T">Type parameter for this edit.</typeparam>
        /// <param name="layout">Explicitly define the layout to use.</param>
        /// <param name="recordId">The internal FileMaker RecordId of the record to be edited.</param>
        /// <param name="input">Object with the updated values.</param>
        /// <returns></returns>
        public Task<IEditResponse> EditAsync<T>(
            string layout,
            int recordId,
            T input) where T : class, new()
        {
            var request = GenerateEditRequest<T>();
            request.Data = input;
            request.Layout = layout;
            request.RecordId = recordId;
            return SendAsync(request);
        }

        /// <summary>
        /// Edit a record.
        /// </summary>
        /// <param name="layout">Explicitly define the layout to use.</param>
        /// <param name="recordId">The internal FileMaker RecordId of the record to be edited.</param>
        /// <param name="editValues">Object with the updated values.</param>
        /// <returns></returns>
        public Task<IEditResponse> EditAsync(
            int recordId,
            string layout,
            Dictionary<string, string> editValues)
        {
            var request = GenerateEditRequest<Dictionary<string, string>>();
            request.Data = editValues;
            request.Layout = layout;
            request.RecordId = recordId;
            return SendAsync(request);
        }
        #endregion

        #region Delete
        /// <summary>
        /// Delete a record utilizing a generic type with the [Table] attribute specifying the layout and the FileMaker RecordId.
        /// </summary>
        /// <typeparam name="T">Class with the [Table] attribute specifying the layout to use.</typeparam>
        /// <param name="recId">The FileMaker RecordId of the record to delete.</param>
        /// <returns></returns>
        public Task<IResponse> DeleteAsync<T>(int recId) where T : class, new()
        {
            var request = GenerateDeleteRequest();
            request.Layout = FileMakerApiClientBase.GetLayoutName(new T());
            request.RecordId = recId;
            return SendAsync(request);
        }

        /// <summary>
        /// Delete a record by id and layout.
        /// </summary>
        public Task<IResponse> DeleteAsync(
            int recId,
            string layout)
        {
            var request = GenerateDeleteRequest();
            request.Layout = layout;
            request.RecordId = recId;
            return SendAsync(request);
        }
        #endregion

        #region "Send Async Methods"
        /// <summary>
        /// Send a Create Record request to the FileMaker API.
        /// </summary>
        public abstract Task<ICreateResponse> SendAsync<T>(ICreateRequest<T> req) where T : class, new();

        /// <summary>
        /// Send a Delete Record request to the FileMaker API.
        /// </summary>
        public abstract Task<IResponse> SendAsync(IDeleteRequest req);

        /// <summary>
        /// Send an Edit Record request to the FileMaker API.
        /// </summary>
        public abstract Task<IEditResponse> SendAsync<T>(IEditRequest<T> req) where T : class, new();

        /// <summary>
        /// Send a Find Record request to the FileMaker API.
        /// </summary>
        public abstract Task<IFindResponse<Dictionary<string, string>>> SendAsync(IFindRequest<Dictionary<string, string>> req);

        /// <summary>
        /// Send a Find Record request to the FileMaker API.
        /// </summary>
        public virtual Task<IEnumerable<T>> SendAsync<T>(
            IFindRequest<T> req) where T : class, new() => SendAsync(req, null, null);

        /// <summary>
        /// Send a Find Record request to the FileMaker API.
        /// </summary>
        public virtual Task<IEnumerable<T>> SendAsync<T>(
            IFindRequest<T> req,
            Func<T, int, object> fmId) where T : class, new() => SendAsync(req, fmId, null);

        /// <summary>
        /// Send a Find Record request to the FileMaker API.
        /// </summary>
        public abstract Task<IEnumerable<T>> SendAsync<T>(
            IFindRequest<T> req,
            Func<T, int, object> fmId,
            Func<T, int, object> modId) where T : class, new();

        #endregion

        /// <summary>
        /// Find a record with utilizing a class instance to define the find request field values.
        /// </summary>
        /// <typeparam name="T">The response type to extract and return.</typeparam>
        /// <param name="layout">The layout to perform the request on.</param>
        /// <param name="req">The dictionary of key/value pairs to find against.</param>
        /// <returns></returns>
        public abstract Task<IEnumerable<T>> FindAsync<T>(string layout, Dictionary<string, string> req);

        /// <summary>
        /// Get a single record by FileMaker RecordId
        /// </summary>
        /// <typeparam name="T">The type to load the data into.</typeparam>
        /// <param name="fileMakerId">The FileMaker RecordId of the record to load.</param>
        /// <param name="fmId">The function to use to map the FileMakerId to the return object.</param>
        /// <returns>A single record matching the FileMaker Record Id.</returns>
        public virtual Task<T> GetByFileMakerIdAsync<T>(int fileMakerId, Func<T, int, object> fmId = null) where T : class, new()
        {
            var layout = GetLayoutName(new T()); // probably a better way
            return GetByFileMakerIdAsync(layout, fileMakerId, fmId);
        }

        /// <summary>
        /// Get a single record by FileMaker RecordId
        /// </summary>
        /// <typeparam name="T">The type to load the data into.</typeparam>
        /// <param name="fileMakerId">The FileMaker RecordId of the record to load.</param>
        /// <param name="fmId">The function to use to map the FileMakerId to the return object.</param>
        /// <param name="fmMod">The function to use to map the FileMaker ModId to the return object.</param>
        /// <returns>A single record matching the FileMaker Record Id.</returns>
        public virtual Task<T> GetByFileMakerIdAsync<T>(int fileMakerId, Func<T, int, object> fmId = null, Func<T, int, object> fmMod = null) where T : class, new()
        {
            var layout = GetLayoutName(new T()); // probably a better way
            return GetByFileMakerIdAsync(layout, fileMakerId, fmId, fmMod);
        }

        /// <summary>
        /// Get a single record by FileMaker RecordId
        /// </summary>
        /// <typeparam name="T">The type to load the data into.</typeparam>
        /// <param name="layout">the layout to execute the request on.</param>
        /// <param name="fileMakerId">The FileMaker RecordId of the record to load.</param>
        /// <param name="fmId">The function to use to map the FileMakerId to the return object.</param>
        /// <returns>A single record matching the FileMaker Record Id.</returns>
        public virtual Task<T> GetByFileMakerIdAsync<T>(string layout, int fileMakerId, Func<T, int, object> fmId = null) where T : class, new()
        {
            return GetByFileMakerIdAsync(layout, fileMakerId, fmId, null);
        }

        /// <summary>
        /// Get a single record by FileMaker RecordId
        /// </summary>
        /// <typeparam name="T">The type to load the data into.</typeparam>
        /// <param name="layout">The layout to execute the request against.</param>
        /// <param name="fileMakerId">The FileMaker RecordId of the record to load.</param>
        /// <param name="fmId">The function to use to map the FileMakerId to the return object.</param>
        /// <param name="fmMod">The function to use to map the FileMaker ModId to the return object.</param>
        /// <returns>A single record matching the FileMaker Record Id.</returns>
        public abstract Task<T> GetByFileMakerIdAsync<T>(string layout, int fileMakerId, Func<T, int, object> fmId = null, Func<T, int, object> fmMod = null) where T : class, new();

        /// <summary>
        /// Set the value of global fields.
        /// // https://fmhelp.filemaker.com/docs/17/en/dataapi/#set-global-fields
        /// </summary>
        /// <param name="baseTable">The base table on which this global field is defined.</param>
        /// <param name="fieldName">The name of the global field to set.</param>
        /// <param name="targetValue">The target value for this global field.</param>
        /// <returns>FileMaker Response</returns>
        public abstract Task<IResponse> SetGlobalFieldAsync(string baseTable, string fieldName, string targetValue);

        #region Container Handling
        /// <summary>
        /// Load the contents of the container data into the attributed property of the model.
        /// </summary>
        /// <typeparam name="T">The type of object to populate.</typeparam>
        /// <param name="instance">Instance of the object that has container data with the ContainerDataForAttribute.</param>
        public virtual async Task ProcessContainer<T>(T instance)
        {
            var ti = typeof(T).GetTypeInfo();
            var props = ti.DeclaredProperties.Where(p => p.GetCustomAttribute<ContainerDataForAttribute>() != null);
            foreach (var prop in props)
            {
                var containerField = prop.GetCustomAttribute<ContainerDataForAttribute>().ContainerField;
                var containerEndPoint = ti.GetDeclaredProperty(containerField).GetValue(instance) as string;

                if (string.IsNullOrEmpty(containerEndPoint))
                {
                    continue;
                }
                else if (!Uri.IsWellFormedUriString(containerEndPoint, UriKind.Absolute))
                {
                    continue;
                }

                var dataBytes = await GetContainerOnClient(containerEndPoint);
                prop.SetValue(instance, dataBytes);
            }
        }

        /// <summary>
        /// Utility method that must be overridden in implementations. Takes a containerfield url and populates a byte array utilizing the instance's http client.
        /// </summary>
        /// <param name="containerEndPoint">The container field to load.</param>
        /// <returns>An array of bytes with the data from the container field.</returns>
        protected abstract Task<byte[]> GetContainerOnClient(string containerEndPoint);

        /// <summary>
        /// Load the contents of the container data into the attributed property of the models.
        /// </summary>
        /// <typeparam name="T">The type of object to populate.</typeparam>
        /// <param name="instances">Collection of objects that have container data with the ContainerDataForAttribute.</param>
        public virtual Task ProcessContainers<T>(IEnumerable<T> instances)
        {
            List<Task> instanceTasks = new List<Task>();

            foreach (var instance in instances)
            {
                instanceTasks.Add(ProcessContainer(instance));
            }

            return Task.WhenAll(instanceTasks);
        }
        #endregion

        #region Container Uploads
        /// <summary>
        /// Puts the contents of the byte array into the specified container field.
        /// </summary>
        /// <param name="layout">The layout to perform this operation on.</param>
        /// <param name="recordId">The FileMaker RecordID of the record we want to update the container on.</param>
        /// <param name="fieldName">Name of the Container Field.</param>
        /// <param name="fileName">The name of the file being inserted into the container field.</param>
        /// <param name="content">The content to be inserted into the container field.</param>
        /// <returns>The FileMaker Server Response from this operation.</returns>
        public virtual Task<IEditResponse> UpdateContainerAsync(
            string layout,
            int recordId,
            string fieldName,
            string fileName,
            byte[] content) => UpdateContainerAsync(layout, recordId, fieldName, fileName, 1, content); // default repetition of 1.

        /// <summary>
        /// Puts the contents of the byte array into the specified container field.
        /// </summary>
        /// <param name="layout">The layout to perform this operation on.</param>
        /// <param name="recordId">The FileMaker RecordID of the record we want to update the container on.</param>
        /// <param name="fieldName">Name of the Container Field.</param>
        /// <param name="fileName">The name of the file being inserted into the container field.</param>
        /// <param name="repetition">Field repetition number.</param>
        /// <param name="content">The content to be inserted into the container field.</param>
        /// <returns>The FileMaker Server Response from this operation.</returns>
        public abstract Task<IEditResponse> UpdateContainerAsync(
            string layout,
            int recordId,
            string fieldName,
            string fileName,
            int repetition,
            byte[] content);
        #endregion

        #region Utility Methods
        /// <summary>
        /// Utility method to get the TableAttribute name to be used for the layout option in the request.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>The specified in the Table Attribute</returns>
        public static string GetLayoutName<T>(T instance)
        {
            string lay;
            try
            {
                var ti = typeof(T).GetTypeInfo();
                lay = ti.GetCustomAttribute<DataContractAttribute>().Name;
            }
            catch
            {
                throw new ArgumentException($"Could not load Layout name from DataContractAttribute on {typeof(T).Name}.");
            }
            return lay;
        }
        #endregion

        /// <summary>
        /// IDisposable
        /// </summary>
        public abstract void Dispose();
    }
}