using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using RestSharp.Extensions.MonoHttp;

namespace IndexTankDotNet
{
    /// <summary>
    ///     A query that can be used to search the documents in an index.
    /// </summary>
    public class Query
    {
        /// <summary>
        ///     Initializes a new instance of the Query class.
        /// </summary>
        /// <param name="queryText">
        ///     The text to search for. Can include optional syntax that allows the targeting of specific
        ///     fields, boolean operators, phrase searching, and more.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if queryText is null.</exception>
        /// <exception cref="ArgumentException">Thrown if queryText is an empty string, or contains only whitespace.</exception>
        public Query(string queryText)
        {
            if (queryText == null) throw new ArgumentNullException("queryText", "The query text is null.");

            if (string.IsNullOrWhiteSpace(queryText)) throw new ArgumentException("The query text is empty.", "queryText");

            QueryText = queryText;

            QueryString = "q=" + HttpUtility.UrlEncode(queryText.ToUTF8(), Encoding.UTF8);
        }

        internal string QueryText { get; }

        internal string QueryString { get; set; }

        /// <summary>
        ///     Allows a given number of documents that satisfy the query be skipped over in the search result. Used for paging
        ///     purposes.
        /// </summary>
        /// <param name="count">The number of result documents to skip.</param>
        /// <returns>Returns the Query.</returns>
        public Query Skip(int count)
        {
            QueryString += "&start=" + count;
            return this;
        }

        /// <summary>
        ///     Used to restrict the number of documents that will be returned from those that satisfy the query. Used for paging
        ///     purposes.
        /// </summary>
        /// <param name="count">The number of result documents to return.</param>
        /// <returns>Returns the Query.</returns>
        public Query Take(int count)
        {
            QueryString += "&len=" + count;
            return this;
        }

        /// <summary>
        ///     Used to indicate which scoring function to use to order the documents that are returned. If this method is not
        ///     called, function 0 will be used by default.
        /// </summary>
        /// <param name="functionNumber">The number of the function to be used.</param>
        /// <returns>Returns the Query.</returns>
        public Query WithScoringFunction(int functionNumber)
        {
            QueryString += "&function=" + functionNumber;
            return this;
        }

        /// <summary>
        ///     Used to indicate that the matched text and the text immediately preceding and following it should be returned along
        ///     with the result.
        /// </summary>
        /// <param name="fields">The names of the fields from which snippets may be retrieved.</param>
        /// <returns>Returns the Query.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if fields is null, or if any one of the field strings in the list is
        ///     null.
        /// </exception>
        public Query WithSnippetFromFields(params string[] fields)
        {
            if (fields == null) throw new ArgumentNullException("fields", "The fields parameters are null.");

            if (fields.Any(f => f == null)) throw new ArgumentNullException("fields", "One or more of the fields are null.");

            var snippetFields = string.Join(",", fields.Where(f => !string.IsNullOrWhiteSpace(f)));

            QueryString += "&snippet=" + HttpUtility.UrlEncode(snippetFields, Encoding.UTF8);
            return this;
        }

        /// <summary>
        ///     Used to indicate that the entire contents of one or more fields should be returned along with the result.
        /// </summary>
        /// <param name="fields">The names of the fields to be retrieved.</param>
        /// <returns>Returns the Query.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if fields is null, or if any one of the field strings in the list is
        ///     null.
        /// </exception>
        public Query WithFields(params string[] fields)
        {
            if (fields == null) throw new ArgumentNullException("fields", "The fields parameters are null.");

            if (fields.Any(f => f == null)) throw new ArgumentNullException("fields", "One or more of the fields are null.");

            var fetchFields = string.Join(",", fields.Where(f => !string.IsNullOrWhiteSpace(f)));

            QueryString += "&fetch=" + HttpUtility.UrlEncode(fetchFields, Encoding.UTF8);
            return this;
        }

        /// <summary>
        ///     Used to indicate that the variables associated with each result document should be returned.
        /// </summary>
        /// <returns>Returns the Query.</returns>
        public Query WithVariables()
        {
            QueryString += "&fetch_variables=true";
            return this;
        }

        /// <summary>
        ///     Used to indicate that the categories associated with each result document should be returned.
        /// </summary>
        /// <returns>Returns the Query.</returns>
        public Query WithCategories()
        {
            QueryString += "&fetch_categories=true";
            return this;
        }

        /// <summary>
        ///     Used to supply an additional variable that can be used by the scoring function. Useful for distance functions used
        ///     in geolocation, among other uses.
        /// </summary>
        /// <param name="variableNumber">A number used to distinguish the variable in the query and in the scoring function..</param>
        /// <param name="value">The value of the query variable.</param>
        /// <returns>Returns the Query.</returns>
        public Query WithQueryVariable(int variableNumber, float value)
        {
            QueryString += "&var" + variableNumber + "=" + value;
            return this;
        }

        /// <summary>
        ///     Used to filter result documents by category, based on the the value of the category to match. May be called more
        ///     than once to specify multiple categories.
        /// </summary>
        /// <param name="category">The name of the category to filter by.</param>
        /// <param name="matches">The values of the specified category that should be matched for a result document to be returned.</param>
        /// <returns>Returns the Query.</returns>
        /// <exception cref="ArgumentNullException">Thrown if category or matches is null, or if any of the match strings is null.</exception>
        /// <exception cref="ArgumentException">Thrown if category is an empty string, or contain only whitespace.</exception>
        public Query WithCategoryFilter(string category, params string[] matches)
        {
            if (category == null) throw new ArgumentNullException("category", "The category is null.");

            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("The category is empty.", "category");

            if (matches == null) throw new ArgumentNullException("matches", "The fields parameters are null.");

            if (matches.Any(f => f == null)) throw new ArgumentNullException("matches", "One or more of the fields are null.");

            var dictionary = new Dictionary<string, string[]> {{category, matches.Where(m => !string.IsNullOrWhiteSpace(m)).ToArray()}};

            QueryString += "&category_filters=" + HttpUtility.UrlEncode(JsonConvert.SerializeObject(dictionary), Encoding.UTF8);
            return this;
        }

        /// <summary>
        ///     Used to filter result documents by a range of variable values. May be called more than once to specify multiple
        ///     variables, or multiple ranges for a single variable.
        /// </summary>
        /// <param name="variableNumber">The number of the variable to filter by.</param>
        /// <param name="lowerBound">The lower bound of the range in which the variable's value should lie.</param>
        /// <param name="upperBound">The upper bound of the range in which the variable's value should lie.</param>
        /// <returns>Returns the Query.</returns>
        public Query WithDocumentVariableFilter(int variableNumber, float lowerBound, float upperBound)
        {
            QueryString += "&filter_docvar" + variableNumber + "=" + lowerBound.ToRequestString() + ":" + upperBound.ToRequestString();

            var parameters = HttpUtility.ParseQueryString(QueryString);

            var queryStringList = (from string key in parameters select string.Concat(key, "=", HttpUtility.UrlEncode(parameters[key]))).ToList();

            QueryString = string.Join("&", queryStringList);

            return this;
        }

        /// <summary>
        ///     Used to filter result documents by indicating that a scoring function calculated result should fall into a
        ///     specified range. May be called more than once to specify multiple functions, or multiple ranges for a single
        ///     function.
        /// </summary>
        /// <param name="functionNumber">The number of the function to filter by.</param>
        /// <param name="lowerBound">The lower bound of the range in which the function's calculated result should lie.</param>
        /// <param name="upperBound">The upper bound of the range in which the function's calculated result should lie.</param>
        /// <returns>Returns the Query.</returns>
        public Query WithFunctionFilter(int functionNumber, double lowerBound, double upperBound)
        {
            QueryString += "&filter_function" + functionNumber + "=" + lowerBound.ToRequestString() + ":" + upperBound.ToRequestString();

            var parameters = HttpUtility.ParseQueryString(QueryString);

            var queryStringList = (from string key in parameters select string.Concat(key, "=", HttpUtility.UrlEncode(parameters[key]))).ToList();

            QueryString = string.Join("&", queryStringList);

            return this;
        }
    }
}