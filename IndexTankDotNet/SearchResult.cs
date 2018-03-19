namespace IndexTankDotNet
{
   using System.Collections.Generic;
   using Newtonsoft.Json;

   /// <summary>
   /// The result returned by a search.
   /// </summary>
   [JsonObject]
   public class SearchResult
   {
      /// <summary>
      /// Gets the total number of documents that satisfy the query, without regard to paging.
      /// </summary>
      [JsonProperty(PropertyName = "matches")]
      public int Matches { get; private set; }

      /// <summary>
      /// Gets the time it took for the search to be executed, in seconds.
      /// </summary>
      [JsonProperty(PropertyName = "search_time")]
      public decimal SearchTime { get; private set; }

      /// <summary>
      /// Gets the query text that was supplied to the original query.
      /// </summary>
      [JsonProperty(PropertyName = "query")]
      public string QueryText { get; private set; }  

      /// <summary>
      /// Gets a string containing a closely related term or phrase that could yield more relevant results, if one exists and if fuzzy searching is enabled on the index; otherwise null.
      /// </summary>
      [JsonProperty(PropertyName = "didyoumean")]
      public string DidYouMeanSuggestion { get; private set; }

      /// <summary>
      /// Gets a collection of ResultDocument objects that contain data about the documents that were matched by the query. If no documents were matched, this will be null.
      /// </summary>
      [JsonProperty(PropertyName = "results")]
      public IList<ResultDocument> ResultDocuments { get; private set; }

      /// <summary>
      /// Gets a nested key/value collection that maps how categories were matched by the result documents. For the outer collection; the key is the name of the category, and the value is another key/value collection. For the second (nested) collection, the key is the value of the category, and the value is the number of times the category value was matched. If no documents are matched, or there are no categories associated with the documents that were matched, this will be null.
      /// </summary>
      [JsonProperty(PropertyName = "facets")]
      public IDictionary<string, Dictionary<string, int>> Facets { get; private set; }
   }
}