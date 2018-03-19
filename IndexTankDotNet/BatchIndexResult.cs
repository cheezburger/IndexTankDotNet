namespace IndexTankDotNet
{
   /// <summary>
   /// Represents a result returned when attempting to index a document using the AddDocuments method of the Index object.
   /// </summary>
   public class BatchIndexResult
   {
      /// <summary>
      /// Gets a value indicating whether whether the document was successfully added to the index.
      /// </summary>
      public bool Added { get; private set; }

      /// <summary>
      /// Gets a message indicating the reason a document was not successfully added to the index. If the document was successfully added, this value should be null.
      /// </summary>
      public string Error { get; private set; }

      /// <summary>
      /// Gets the document.
      /// </summary>
      public Document Document { get; internal set; }
   }
}