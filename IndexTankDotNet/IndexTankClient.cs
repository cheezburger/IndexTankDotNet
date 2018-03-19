using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;

namespace IndexTankDotNet
{
    /// <summary>
    ///     A client which allows programmatic access to indexes in an IndexTank account.
    /// </summary>
    public class IndexTankClient
    {
        private readonly RestClient restClient;

        /// <summary>
        ///     Initializes a new instance of the IndexTankClient class using a private API URL string.
        ///     <para>Same as calling: new IndexTankClient(new Uri(privateUrl))</para>
        /// </summary>
        /// <param name="privateUrl">A private URL to an IndexTank account.</param>
        /// <seealso cref="IndexTankClient(Uri)" />
        /// <exception cref="ArgumentNullException">Thrown if orivateUrl is null.</exception>
        public IndexTankClient(string privateUrl) : this(new Uri(privateUrl))
        {
            if (privateUrl == null) throw new ArgumentNullException("privateUrl", "The private URL is null.");
        }

        /// <summary>
        ///     Initializes a new instance of the IndexTankClient class using a URI to a private API URL.
        /// </summary>
        /// <param name="privateUri">A Uri that indicates a provate URL to an IndexTank account.</param>
        /// <exception cref="ArgumentNullException">Thrown if privateUri is null.</exception>
        public IndexTankClient(Uri privateUri)
        {
            if (privateUri == null) throw new ArgumentNullException("privateUri", "The URI is null.");

            restClient = new RestClient(privateUri.ToString());
            var userInfo = privateUri.UserInfo;
            var password = userInfo.Split(':')[1];

            restClient.Authenticator = new HttpBasicAuthenticator(string.Empty, password);
        }

        /// <summary>
        ///     Creates a new index.
        /// </summary>
        /// <param name="indexName">The name of the index to create.</param>
        /// <param name="enablePublicApi">Indicates whether the public API should be enabled for the index.</param>
        /// <param name="enableSuggestions">
        ///     Indicates whether fuzzy searching should be enabled for the index. If this value is
        ///     true, but fuzzy searching is not supported for the index, a NotSupportedException will be thrown.
        /// </param>
        /// <returns>Returns the newly created index.</returns>
        /// <exception cref="ArgumentNullException">Thrown if indexName is null.</exception>
        /// <exception cref="ArgumentException">Thrown if indexName is an empty string, or contains only whitespace.</exception>
        /// <exception cref="FormatException">
        ///     Thrown if indexName contains one or more characters that is not a letter, digit, or
        ///     underscore (_).
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     Thrown if enableSuggestions = true, but fuzzy searching is not supported for
        ///     the index.
        /// </exception>
        public Index CreateIndex(string indexName, bool enablePublicApi = false, bool enableSuggestions = false)
        {
            if (indexName == null) throw new ArgumentNullException("indexName", "The index name is null.");

            if (string.IsNullOrWhiteSpace(indexName)) throw new ArgumentException("The index name is empty.", "indexName");

            if (indexName.ToCharArray().Any(c => !(char.IsLetterOrDigit(c) || c == '_')))
                throw new FormatException(
                    "The index name contains one or more characters that is not a letter, digit, or underscore (_).");

            var request = new RestRequest(string.Format("{0}/{1}", ResourceUri.METADATA_V1_URI, indexName), Method.PUT)
            {
                RequestFormat = DataFormat.Json
            };

            if (enablePublicApi) request.AddParameter("text/json", "{\"public_search\": true}", ParameterType.RequestBody);

            if (enableSuggestions) request.AddParameter("text/json", "{\"did_you_mean\": true}", ParameterType.RequestBody);

            var response = Execute(request);

            Index index = null;

            if (response.StatusCode == HttpStatusCode.Created)
            {
                index = JsonConvert.DeserializeObject<Index>(response.Content);

                if (enableSuggestions && !index.AreSuggestionsEnabled)
                    throw new NotSupportedException(
                        "Fuzzy searching is a beta feature of the API, and is not supported for this index. Contact IndexTank support to allow fuzzy searching to be enabled for your indexes.");

                index.Name = indexName;
                index.IndexTankClient = this;
            }

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                var updatedIndex = GetIndex(indexName); // note: this is here only while fuzzy searching is in beta.

                if (enableSuggestions && !updatedIndex.AreSuggestionsEnabled)
                    throw new NotSupportedException(
                        "Fuzzy searching is a beta feature of the API, and is not supported for this index. Contact IndexTank support to allow fuzzy searching to be enabled for your indexes.");

                return null;
            }

            if (index == null)
                throw new IndexTankProtocolException(
                    "An unexpected error occurred.",
                    response.ErrorException,
                    response.StatusCode);

            return index;
        }

        /// <summary>
        ///     Gets an index by name.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>Returns an Index.</returns>
        /// <exception cref="IndexTankProtocolException">
        ///     Thrown if an unexpected error occurs that prevents the index from being
        ///     retrieved.
        /// </exception>
        public Index GetIndex(string indexName)
        {
            var request = new RestRequest(string.Format("{0}/{1}", ResourceUri.METADATA_V1_URI, indexName), Method.GET);

            var response = Execute(request);

            var index = JsonConvert.DeserializeObject<Index>(response.Content);

            if (index.Status == "ERROR")
                throw new IndexTankProtocolException("An unexpected error occurred.", response.ErrorException,
                    response.StatusCode);

            index.Name = indexName;
            index.IndexTankClient = this;

            return index;
        }

        /// <summary>
        ///     Gets all indexes on an account.
        /// </summary>
        /// <returns>Returns a collection of indexes.</returns>
        /// ///
        /// <exception cref="IndexTankProtocolException">
        ///     Thrown if an unexpected error occurs that prevents the indexes from being
        ///     retrieved.
        /// </exception>
        public IEnumerable<Index> GetIndexes()
        {
            var request = new RestRequest(ResourceUri.METADATA_V1_URI, Method.GET);

            var response = Execute(request);

            try
            {
                IDictionary<string, Index> indexEntries =
                    JsonConvert.DeserializeObject<Dictionary<string, Index>>(response.Content);

                var indexes = new List<Index>();

                foreach (var indexEntry in indexEntries)
                {
                    var index = indexEntry.Value;
                    index.Name = indexEntry.Key;
                    index.IndexTankClient = this;

                    indexes.Add(index);
                }

                return indexes;
            }
            catch (JsonSerializationException)
            {
                throw new IndexTankProtocolException("An unexpected error occurred. Please ensure the Private URL you supplied is correct.", null, response.StatusCode);
            }
        }

        /// <summary>
        ///     Deletes an index by name.
        /// </summary>
        /// <param name="indexName">The name of the index to delete.</param>
        /// <returns>true if the deletion succeeds; otherwise false.</returns>
        public bool DeleteIndex(string indexName)
        {
            var request = new RestRequest(string.Format("{0}/{1}", ResourceUri.METADATA_V1_URI, indexName), Method.DELETE);

            var response = Execute(request);

            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent;
        }

        internal IRestResponse<T> Execute<T>(RestRequest request) where T : new()
        {
            HandleRequest(request);
            var response = restClient.Execute<T>(request);
            HandleResponse(response);

            return response;
        }

        internal IRestResponse Execute(RestRequest request)
        {
            HandleRequest(request);
            var response = restClient.Execute(request);
            HandleResponse(response);

            return response;
        }

        private static void HandleRequest(IRestRequest request)
        {
            var byteCount =
                request.Parameters.Where(p => p.Type == ParameterType.RequestBody).Sum(
                    parameter => Encoding.UTF8.GetByteCount(parameter.Value.ToString()));

            if (byteCount > 1000000)
                throw new InvalidOperationException(
                    string.Format("The size of the request exceeds 1 MB. The request size is {0} MB.",
                        Convert.ToDecimal(byteCount)));
        }

        private static void HandleResponse(IRestResponse response)
        {
            if (response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Created ||
                response.StatusCode == HttpStatusCode.NoContent)
                return;

            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new IndexTankProtocolException("Authorization failed.", response.ErrorException,
                        HttpStatusCode.Unauthorized);
                case HttpStatusCode.Conflict:
                {
                    if (new StackTrace().GetFrame(2).GetMethod().Name == "CreateIndex")
                        throw new IndexTankApiException("There are too many indexes for this account.",
                            HttpStatusCode.Conflict);

                    throw new IndexTankApiException(
                        "The index is not yet initialized. Assure that the index's Started property is true before accessing it.",
                        HttpStatusCode.Conflict);
                }

                case HttpStatusCode.ServiceUnavailable:
                    throw new IndexTankApiException("The IndexTank service is unavailable.",
                        HttpStatusCode.ServiceUnavailable);

                case HttpStatusCode.NotFound:
                    throw new IndexTankApiException("The index was not found.", HttpStatusCode.NotFound);

                case HttpStatusCode.BadRequest:
                {
                    var methodName = new StackTrace().GetFrame(2).GetMethod().Name;

                    if (methodName == "AddFunction")
                        throw new IndexTankApiException("The function definition is malformed. Check your syntax.",
                            HttpStatusCode.Conflict);

                    if (methodName == "AddDocument" || methodName == "UpdateVariables")
                        throw new IndexTankApiException(
                            "Invalid argument. Make sure you have not specified a variable number that exceeds the number that is allowed for the current index.",
                            HttpStatusCode.Conflict);

                    if (methodName == "Search") throw new IndexTankApiException("The query is invalid.", HttpStatusCode.Conflict);

                    throw new IndexTankApiException("Invalid or missing argument.", HttpStatusCode.BadRequest);
                }
            }

            switch (response.ResponseStatus)
            {
                case ResponseStatus.Error:
                    throw new IndexTankProtocolException(response.ErrorMessage, response.ErrorException, response.StatusCode);

                case ResponseStatus.TimedOut:
                    throw new IndexTankProtocolException("The request timed out.", response.ErrorException,
                        response.StatusCode);

                case ResponseStatus.None:
                    throw new IndexTankProtocolException("The server did not return a response.", response.ErrorException,
                        response.StatusCode);
            }
        }
    }
}