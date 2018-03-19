namespace IndexTankDotNet
{
   using System.Collections.Generic;
   using Newtonsoft.Json;

   /// <summary>
   /// Contains data about a document that was matched by a query.
   /// </summary>
   [JsonObject]
   public class ResultDocument
   {
      /// <summary>
      /// Gets the document identifier.
      /// </summary>
      [JsonProperty(PropertyName = "docid")]
      public string DocumentId { get; private set; }

      /// <summary>
      /// Gets the query-specific document relevance score.
      /// </summary>
      [JsonProperty(PropertyName = "query_relevance_score")]
      public float QueryRelevanceScore { get; private set; }

      /// <summary>
      /// Gets a key/value collection of snippets containing the matched text and the text immediately preceding and following it, if they were requested by the query; where the key is the field name in which the text was found, and the value is the snippet.
      /// </summary>
      public IDictionary<string, string> Snippets { get; internal set; }

      /// <summary>
      /// Gets a key/value collection of categories associated with the result document, if they were requested by the query; where the key is the category name, and the value is the category value.
      /// </summary>
      public IDictionary<string, string> Categories { get; internal set; }

      /// <summary>
      /// Gets a key/value collection of the result document fields that were requested by the query; where the key is the name of the field, and the value is the entire content from the field.
      /// </summary>
      public IDictionary<string, string> Fields { get; internal set; }

      /// <summary>
      /// Gets a list of values of the variable associated with the result document if they were requested by the query, where the index of the variable in the list is the variable number.
      /// </summary>
      public IList<float> Variables { get; internal set; }
   }
}