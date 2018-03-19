using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Extensions.MonoHttp;

namespace IndexTankDotNet
{
    /// <summary>
    ///     An object that contains documents that can be searched.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Index
    {
        [JsonConstructor]
        private Index()
        {
        }

        internal IndexTankClient IndexTankClient { private get; set; }

        /// <summary>
        ///     Gets the index name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        ///     Gets the status of a retrieved index.
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public string Status { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the index has been started on the server. A value of false usually means that the
        ///     index has been recently created and is not yet available for use.
        /// </summary>
        [JsonProperty(PropertyName = "started", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsStarted { get; private set; }

        /// <summary>
        ///     Gets an alphanumeric code that uniquely identifies the index under a given name. If an index is deleted and a new
        ///     one is created with the same name, it will have a different code.
        /// </summary>
        [JsonProperty(PropertyName = "code")]
        public string Code { get; private set; }

        /// <summary>
        ///     Gets the date and time the index was created on the server.
        /// </summary>
        [JsonProperty(PropertyName = "creation_time")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime CreationTime { get; private set; }

        /// <summary>
        ///     Gets the number of documents in the index. Size is not updated in real time, so the value may be up to a minute
        ///     old.
        /// </summary>
        [JsonProperty(PropertyName = "size")]
        public int Size { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the public search API has been enabled for this index.
        /// </summary>
        [JsonProperty(PropertyName = "public_search")]
        public bool IsPublicApiEnabled { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether "fuzzy" search suggestions have been enabled for this index.
        /// </summary>
        [JsonProperty(PropertyName = "did_you_mean")]
        public bool AreSuggestionsEnabled { get; private set; }

        /// <summary>
        ///     Allows the enabling and disabling of various options in the index.
        /// </summary>
        /// <param name="enablePublicApi">Enables or disables the public API for the index.</param>
        /// <param name="enableSuggestions">
        ///     Enables or disables the ability to perform fuzzy searching against the index. If this
        ///     value is true, but fuzzy searching is not supported for the index, a NotSupportedException will be thrown.
        /// </param>
        /// <returns>true if the update succeeded; otherwise false.</returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if enableSuggestions = true, but fuzzy searching is not supported for
        ///     the index.
        /// </exception>
        public bool UpdateIndex(bool enablePublicApi, bool enableSuggestions)
        {
            // wrapper for CreateIndex(), but lets you change the body
            return IndexTankClient.CreateIndex(Name, enablePublicApi, enableSuggestions) == null;
        }

        /// <summary>
        ///     Adds a document to the index.
        /// </summary>
        /// <param name="document">The document to add to the index.</param>
        /// <returns>true if the addition succeeded; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the document is null.</exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the document has no fields, or if the combined size of all fields
        ///     in the document exceeds 100 kbytes.
        /// </exception>
        public bool AddDocument(Document document)
        {
            if (document == null) throw new ArgumentNullException("document", "The document is null.");

            if (document.Fields == null)
                throw new InvalidOperationException(
                    "The document has no fields. A document must have at least one field before adding it to an index.");

            var totalFieldBytes = document.Fields.Sum(field => Encoding.UTF8.GetByteCount(field.Value));

            if (totalFieldBytes > 100000)
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "The combined size of all field values is {0} bytes. The combined size may not exceed 100kbytes.", totalFieldBytes));

            var request = new RestRequest(string.Format("{0}/{1}{2}", ResourceUri.METADATA_V1_URI, Name, ResourceUri.DOCS_URI), Method.PUT)
            {
                RequestFormat = DataFormat.Json
            };

            request.AddBody(document);

            var response = IndexTankClient.Execute(request);

            return response.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        ///     Allows the addition of several documents at once to the index.
        /// </summary>
        /// <param name="documents">The collection of documents to add to the index.</param>
        /// <returns>Returns a BatchIndexResultCollection.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the collection of documents to add is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the collection of documents to add is empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the overall size of the request exceeds 1MB.</exception>
        public BatchIndexResultCollection AddDocuments(IEnumerable<Document> documents)
        {
            if (documents == null) throw new ArgumentNullException("documents", "The list of documents is null.");

            var docList = documents.ToList();

            if (!docList.Any()) throw new ArgumentException("The list of documents is empty.", "documents");

            var request = new RestRequest(string.Format("{0}/{1}{2}", ResourceUri.METADATA_V1_URI, Name, ResourceUri.DOCS_URI), Method.PUT)
            {
                RequestFormat = DataFormat.Json
            };

            request.AddBody(documents);

            var response = IndexTankClient.Execute<List<BatchIndexResult>>(request);

            var resultsList = response.Data;
            var results = new BatchIndexResultCollection();

            for (var i = 0; i < docList.Count; i++)
            {
                resultsList[i].Document = docList[i];
                results.Add(resultsList[i]);
            }

            return results;
        }

        /// <summary>
        ///     Allows the promotion of a document to the top of the results page for a query that uses the given text.
        /// </summary>
        /// <param name="documentId">The identifier of the document to promote.</param>
        /// <param name="queryText">The text of the query for which the document should be promoted.</param>
        /// <returns>true if the promotion succeeded; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either documentId or queryText are null.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown if either documentId or queryText are empty strings, or contain only
        ///     whitespace.
        /// </exception>
        public bool PromoteDocument(string documentId, string queryText)
        {
            if (documentId == null) throw new ArgumentNullException("documentId", "The document identifier is null.");

            if (string.IsNullOrWhiteSpace(documentId)) throw new ArgumentException("The document identifier is empty.", "documentId");

            if (queryText == null) throw new ArgumentNullException("queryText", "The query text is null.");

            if (string.IsNullOrWhiteSpace(queryText)) throw new ArgumentException("The query text is empty.", "queryText");

            var request = new RestRequest(string.Format("{0}/{1}{2}", ResourceUri.METADATA_V1_URI, Name, ResourceUri.PROMOTE_URI), Method.PUT);

            request.AddParameter("text/json", "{ \"docid\" : \"" + documentId + "\", \"query\" : \"" + queryText + "\" }",
                ParameterType.RequestBody);

            var response = IndexTankClient.Execute(request);

            return response.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        ///     Deletes a document by its identifier.
        /// </summary>
        /// <param name="documentId">The document identifier.</param>
        /// <returns>true if the deletion succeeded; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if documentId is null.</exception>
        /// <exception cref="ArgumentException">Thrown if documentId is empty, or contans only whitespace.</exception>
        public bool DeleteDocument(string documentId)
        {
            if (documentId == null) throw new ArgumentNullException("documentId", "The document identifier is null.");

            if (string.IsNullOrWhiteSpace(documentId)) throw new ArgumentException("The document identifier is empty.", "documentId");

            var request =
                new RestRequest(
                    string.Format("{0}/{1}{2}?docid={3}", ResourceUri.METADATA_V1_URI, Name, ResourceUri.DOCS_URI, HttpUtility.UrlEncode(documentId, Encoding.UTF8)), Method.DELETE);

            var response = IndexTankClient.Execute(request);

            return response.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        ///     Allows the deletion of several documents at once from the index.
        /// </summary>
        /// <param name="documentIds">An array of document identifiers corresponding to the documents to be deleted.</param>
        /// <returns>Returns a BatchDeleteResultCollection.</returns>
        /// <exception cref="ArgumentNullException">Thrown if documentIds is null.</exception>
        /// <exception cref="ArgumentException">Thrown if documentIds contains no elements.</exception>
        public BatchDeleteResultCollection DeleteDocuments(IEnumerable<string> documentIds)
        {
            if (documentIds == null) throw new ArgumentNullException("documentIds", "The list of document identifiers is null.");

            var idList = documentIds.ToList();

            if (!idList.Any()) throw new ArgumentException("The list of document identifiers is empty.", "documentIds");

            var queryString = "?docid=" + string.Join("&docid=", idList.Select(HttpUtility.UrlEncode));

            var request = new RestRequest(
                string.Format("{0}/{1}{2}{3}", ResourceUri.METADATA_V1_URI, Name, ResourceUri.DOCS_URI, queryString),
                Method.DELETE);

            var response = IndexTankClient.Execute<List<BatchDeleteResult>>(request);

            var resultsList = response.Data;
            var results = new BatchDeleteResultCollection();

            for (var i = 0; i < idList.Count; i++)
            {
                resultsList[i].DocumentId = idList[i];
                results.Add(resultsList[i]);
            }

            return results;
        }

        /// <summary>
        ///     Allows the batch deletion of documents that match the supplied query from the index.
        /// </summary>
        /// <param name="query">
        ///     A Query containing the text to search for, along with several other optional criteria which may be
        ///     supplied by the Query object.
        /// </param>
        /// <returns>true if the matched documents were successfully deleted; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if query is null.</exception>
        public bool DeleteDocuments(Query query)
        {
            if (query == null) throw new ArgumentNullException("query", "The query is null.");

            if (query.QueryString.Contains("&len="))
                throw new NotSupportedException(
                    "Calling the Take method on a Query used for deleting is currently not supported.");

            var request =
                new RestRequest(string.Format("{0}/{1}{2}?{3}", ResourceUri.METADATA_V1_URI, Name, ResourceUri.SEARCH_URI, query.QueryString),
                    Method.DELETE);

            var response = IndexTankClient.Execute(request);

            return response.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        ///     Gets a key/value collection of the scoring functions associated with the index, where the key is the function's
        ///     position in the index, and the value is the function's definition.
        /// </summary>
        /// <returns>Returns the key/value collection of scoring functions.</returns>
        public IDictionary<int, string> GetFunctions()
        {
            var request = new RestRequest(string.Format("{0}/{1}{2}", ResourceUri.METADATA_V1_URI, Name, ResourceUri.FUNCTIONS_URI), Method.GET);

            var response = IndexTankClient.Execute<Dictionary<string, string>>(request);

            if (response.Data == null)
                throw new IndexTankProtocolException("An unexpected error occurred.", response.ErrorException,
                    response.StatusCode);

            return response.Data.ToDictionary(f => Convert.ToInt32(f.Key, CultureInfo.InvariantCulture), f => f.Value);
        }

        /// <summary>
        ///     Adds a scoring function to the index for custom sorting of search results.
        /// </summary>
        /// <param name="functionNumber">The position of the function in the index.</param>
        /// <param name="definition">
        ///     The definition of the function's formula. The definition uses special syntax which is
        ///     described in the IndexTank documentation.
        /// </param>
        /// <returns>true if the addition succeeded; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the definition is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the definition is an empty string, or consists of only whitespace.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if functionNumber is less than zero.</exception>
        public bool AddFunction(int functionNumber, string definition)
        {
            if (functionNumber < 0)
                throw new ArgumentOutOfRangeException(
                    "functionNumber",
                    functionNumber,
                    "The function number is less than zero.");

            if (definition == null) throw new ArgumentNullException("definition", "The function definition is null");

            if (string.IsNullOrWhiteSpace(definition)) throw new ArgumentException("The function definition is empty.", "definition");

            var request =
                new RestRequest(
                    string.Format("{0}/{1}{2}/{3}", ResourceUri.METADATA_V1_URI, Name, ResourceUri.FUNCTIONS_URI, functionNumber),
                    Method.PUT)
                {
                    RequestFormat = DataFormat.Json
                };

            request.AddParameter("text/json", "{\"definition\" : \"" + definition + "\"}", ParameterType.RequestBody);

            var response = IndexTankClient.Execute(request);

            return response.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        ///     Deletes a function at the specified position.
        /// </summary>
        /// <param name="functionNumber">The position of the function to delete.</param>
        /// <returns>true if the deletion succeeded; otherwise false.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if functionNumber is less than zero.</exception>
        public bool DeleteFunction(int functionNumber)
        {
            if (functionNumber < 0)
                throw new ArgumentOutOfRangeException(
                    "functionNumber",
                    functionNumber,
                    "The function number is less than zero.");

            var request =
                new RestRequest(string.Format("{0}/{1}{2}/{3}", ResourceUri.METADATA_V1_URI, Name, ResourceUri.FUNCTIONS_URI, functionNumber),
                    Method.DELETE);

            var response = IndexTankClient.Execute(request);

            return response.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        ///     Quickly updates the variables associated with an existing document, without having to resend the entire document.
        ///     Updates made using this method do not count toward your account's indexing limits.
        /// </summary>
        /// <param name="documentId">The identifier of the document whose variables are to be updated.</param>
        /// <param name="variables">
        ///     A key/value collection of the variables to update, where the key is the position of the
        ///     variable, and the value is the value of the variable.
        /// </param>
        /// <returns>true if the update succeeded; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either documentId or variables are null.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown if documentId is an empty string, or consists of only whitespace; or if the
        ///     list of variables is empty.
        /// </exception>
        public bool UpdateVariables(string documentId, IDictionary<int, float> variables)
        {
            if (documentId == null) throw new ArgumentNullException("documentId", "The document identifier is null.");

            if (string.IsNullOrWhiteSpace(documentId)) throw new ArgumentException("The document identifier is empty.", "documentId");

            if (variables == null) throw new ArgumentNullException("variables", "The list of variables is null.");

            if (!variables.Any()) throw new ArgumentException("The list of variables is empty.", "variables");

            var request = new RestRequest(string.Format("{0}/{1}{2}", ResourceUri.METADATA_V1_URI, Name, ResourceUri.VARIABLES_URI), Method.PUT)
            {
                RequestFormat = DataFormat.Json
            };

            var document = new Document(documentId);

            foreach (var variable in variables) document.AddVariable(variable.Key, variable.Value);

            request.AddBody(document);

            var response = IndexTankClient.Execute(request);

            return response.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        ///     Quickly updates a variable associated with an existing document, without having to resend the entire document.
        ///     Updates made using this method do not count toward your account's indexing limits.
        /// </summary>
        /// <param name="documentId">The identifier of the document upon which the variable is to be updated.</param>
        /// <param name="variableNumber">The position of the variable to be updated.</param>
        /// <param name="value">The new value to be assigned to the variable.</param>
        /// <returns>true if the update succeeded; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if documentId is null.</exception>
        /// <exception cref="ArgumentException">Thrown if documentId is an empty string, or consists of only whitespace.</exception>
        public bool UpdateVariable(string documentId, int variableNumber, float value)
        {
            return UpdateVariables(documentId, new Dictionary<int, float> {{variableNumber, value}});
        }

        /// <summary>
        ///     Quickly updates the categories associated with an existing document, without having to resend the entire document.
        ///     Updates made using this method do not count toward your account's indexing limits.
        /// </summary>
        /// <param name="documentId">The identifier of the document whose categories are to be updated.</param>
        /// <param name="categories">
        ///     A key/value collection of the categories to update, where the key is the name of the category,
        ///     and the value is the value of the category.
        /// </param>
        /// <returns>true if the update succeeded; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either documentId or categories are null.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown if documentId is an empty string, or consists of only whitespace; or if the
        ///     list of categories is empty.
        /// </exception>
        public bool UpdateCategories(string documentId, IDictionary<string, string> categories)
        {
            if (documentId == null) throw new ArgumentNullException("documentId", "The document identifier is null.");

            if (string.IsNullOrWhiteSpace(documentId)) throw new ArgumentException("The document identifier is empty.", "documentId");

            if (categories == null) throw new ArgumentNullException("categories", "The list of categories is null.");

            if (!categories.Any()) throw new ArgumentException("The list of categories is empty.", "categories");

            var request = new RestRequest(string.Format("{0}/{1}{2}", ResourceUri.METADATA_V1_URI, Name, ResourceUri.CATEGORIES_URI), Method.PUT)
            {
                RequestFormat = DataFormat.Json
            };

            var document = new Document(documentId);

            foreach (var category in categories) document.AddCategory(category.Key, category.Value);

            request.AddBody(document);

            var response = IndexTankClient.Execute(request);

            return response.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        ///     Quickly updates a category associated with an existing document, without having to resend the entire document.
        ///     Updates made using this method do not count toward your account's indexing limits.
        /// </summary>
        /// <param name="documentId">The identifier of the document upon which the category is to be updated.</param>
        /// <param name="categoryName">The name of the category to be updated.</param>
        /// <param name="value">The new value to be assigned to the category.</param>
        /// <returns>true if the update succeeded; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either documentId, categoryName, or value are null.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown if either documentId or categoryName is an empty string, or consists of only
        ///     whitespace.
        /// </exception>
        public bool UpdateCategory(string documentId, string categoryName, string value)
        {
            if (categoryName == null) throw new ArgumentNullException("categoryName", "The category name is null.");

            return UpdateCategories(documentId, new Dictionary<string, string> {{categoryName, value}});
        }

        /// <summary>
        ///     Performs a search against the index using a complex query.
        /// </summary>
        /// <param name="query">
        ///     A Query containing the text to search for, along with several other optional criteria which may be
        ///     supplied by the Query object.
        /// </param>
        /// <returns>Returns a SearchResult.</returns>
        /// <exception cref="ArgumentNullException">Thrown if query is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if the sum of the arguments passed to the Skip and Take methods of
        ///     the supplied Query object exceeds 5000.
        /// </exception>
        public SearchResult Search(Query query)
        {
            if (query == null) throw new ArgumentNullException("query", "The query is null.");

            var parameters = HttpUtility.ParseQueryString(query.QueryString);
            var start = Convert.ToInt32(parameters["start"], CultureInfo.InvariantCulture);
            var len = Convert.ToInt32(parameters["len"], CultureInfo.InvariantCulture);

            if (start + len > 5000)
                throw new ArgumentOutOfRangeException("query", start + len,
                    "The sum of the arguments supplied to the Skip and Take methods of the query exceeds 5,000.");

            var request =
                new RestRequest(string.Format("{0}/{1}{2}?{3}", ResourceUri.METADATA_V1_URI, Name, ResourceUri.SEARCH_URI, query.QueryString),
                    Method.GET);

            var response = IndexTankClient.Execute(request);
            var searchResult = JsonConvert.DeserializeObject<SearchResult>(response.Content);

            var searchResponse = JObject.Parse(response.Content);

            foreach (var resultDocument in searchResult.ResultDocuments)
            {
                var document = resultDocument;
                IEnumerable<JProperty> properties =
                    searchResponse["results"].Children().Where(j => j.Value<string>("docid") == document.DocumentId).Children
                        ().Select(t => (JProperty) t).ToList();

                IEnumerable<JProperty> snippetProperties =
                    properties.Where(p => p.Name.StartsWith("snippet_", StringComparison.OrdinalIgnoreCase)).ToList();

                IEnumerable<JProperty> fetchFieldProperties =
                    properties.Where(p => !p.Name.StartsWith("snippet_", StringComparison.OrdinalIgnoreCase)
                                          && !p.Name.StartsWith("variable_", StringComparison.OrdinalIgnoreCase)
                                          && !p.Name.StartsWith("category_", StringComparison.OrdinalIgnoreCase)
                                          && !p.Name.StartsWith("query_relevance_score", StringComparison.OrdinalIgnoreCase)
                                          && !p.Name.StartsWith("docid", StringComparison.OrdinalIgnoreCase)).ToList();

                IEnumerable<JProperty> variableProperties =
                    properties.Where(p => p.Name.StartsWith("variable_", StringComparison.OrdinalIgnoreCase)).ToList();

                IEnumerable<JProperty> categoryProperties =
                    properties.Where(p => p.Name.StartsWith("category_", StringComparison.OrdinalIgnoreCase)).ToList();

                if (snippetProperties.Any()) resultDocument.Snippets = new Dictionary<string, string>();

                if (categoryProperties.Any()) resultDocument.Categories = new Dictionary<string, string>();

                if (fetchFieldProperties.Any()) resultDocument.Fields = new Dictionary<string, string>();

                if (variableProperties.Any()) resultDocument.Variables = new List<float>(new float[variableProperties.Count()]);

                foreach (var property in snippetProperties)
                {
                    var key =
                        property.Name.Substring(property.Name.LastIndexOf("_", StringComparison.OrdinalIgnoreCase) + 1);
                    var value = property.Value.ToString();

                    resultDocument.Snippets.Add(key, value);
                }

                foreach (var property in fetchFieldProperties)
                {
                    var key = property.Name;
                    var value = property.Value.ToString();

                    resultDocument.Fields.Add(key, value);
                }

                foreach (var property in variableProperties)
                {
                    var key =
                        Convert.ToInt32(
                            property.Name.Substring(property.Name.LastIndexOf("_", StringComparison.OrdinalIgnoreCase) + 1),
                            CultureInfo.InvariantCulture);
                    var value = Convert.ToSingle(property.Value.ToString(), CultureInfo.CurrentCulture);

                    resultDocument.Variables[key] = value;
                }

                foreach (var property in categoryProperties)
                {
                    var key =
                        property.Name.Substring(property.Name.LastIndexOf("_", StringComparison.OrdinalIgnoreCase) + 1);
                    var value = property.Value.ToString();

                    resultDocument.Categories.Add(key, value);
                }
            }

            //Thread.Sleep(500);  // uncomment this line to run timeout tests in SearchTests.cs

            return searchResult;
        }

        /// <summary>
        ///     Performs a search against the index using a complex query.
        /// </summary>
        /// <param name="query">
        ///     A Query containing the text to search for, along with several other optional criteria which may be
        ///     supplied by the Query object.
        /// </param>
        /// <param name="timeOutMilliseconds">The desired timeout in milliseconds.</param>
        /// <returns>Returns a SearchResult.</returns>
        /// <exception cref="ArgumentNullException">Thrown if query is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if the sum of the arguments passed to the Skip and Take methods of
        ///     the supplied Query object exceeds 5000; or if timeOutMilliseconds is less than or equal to zero.
        /// </exception>
        /// <exception cref="TimeoutException">Thrown if the method does not return before the supplied timeout elapses.</exception>
        public SearchResult Search(Query query, int timeOutMilliseconds)
        {
            if (timeOutMilliseconds <= 0) throw new ArgumentOutOfRangeException("timeOutMilliseconds", timeOutMilliseconds, "Timeout is less than or equal to zero.");

            var result = TimeOutRunner.Invoke(() => Search(query), TimeSpan.FromMilliseconds(timeOutMilliseconds));

            return result;
        }

        /// <summary>
        ///     Performs a search against the index using a complex query. Automatically searches across all specified fields
        ///     without requiring any special syntax in the query text.
        ///     <para>Same as calling: Search(new Query("field1:queryText OR field2:queryText OR field3:queryText ... "))</para>
        /// </summary>
        /// <param name="query">
        ///     A Query containing the text to search for, along with several other optional criteria which may be
        ///     supplied by the Query object.
        /// </param>
        /// <param name="fieldsToSearch">The fields to search in.</param>
        /// <returns>Returns the Query.</returns>
        /// <seealso cref="Search(Query)" />
        /// <exception cref="ArgumentNullException">Thrown if query or fields is null, or if any of the strings in fields is null.</exception>
        public SearchResult Search(Query query, params string[] fieldsToSearch)
        {
            if (query == null) throw new ArgumentNullException("query", "The query is null");

            if (fieldsToSearch == null) throw new ArgumentNullException("fieldsToSearch", "The fields list is null.");

            if (fieldsToSearch.Any(f => f == null))
                throw new ArgumentNullException("fieldsToSearch",
                    "One or more of the strings contained in the field list is null.");

            var parameters = HttpUtility.ParseQueryString(query.QueryString);

            var newQueryText = string.Empty;

            foreach (var field in fieldsToSearch.Where(field => !string.IsNullOrWhiteSpace(field)))
                if (field == fieldsToSearch.Last())
                    newQueryText += field + ":" + query.QueryText;
                else
                    newQueryText += field + ":" + query.QueryText + " OR ";

            parameters["q"] = newQueryText;

            query.QueryString = string.Join("&", parameters);

            return Search(query);
        }

        /// <summary>
        ///     Performs a search against the index using simple search text.
        ///     <para>Same as calling: Search(new Query(querytext))</para>
        /// </summary>
        /// <param name="queryText">The text to search for.</param>
        /// <returns>Returns a SearchResult.</returns>
        /// <seealso cref="Search(Query)" />
        public SearchResult Search(string queryText)
        {
            return Search(new Query(queryText));
        }
    }
}