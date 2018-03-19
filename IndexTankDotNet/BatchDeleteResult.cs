namespace IndexTankDotNet
{
   /// <summary>
   /// Represents a result returned when attempting to delete a document using the DeleteDocuments method of the Index object.
   /// </summary>
   public class BatchDeleteResult
   {
      /// <summary>
      /// Gets a value indicating whether the document was successfully deleted from the index.
      /// </summary>
      public bool Deleted { get; private set; }

      /// <summary>
      /// Gets a message indicating the reason a document was not successfully deleted from the index. If the document was successfully deleted, this value should be null.
      /// </summary>
      public string Error { get; private set; }

      /// <summary>
      /// Gets the document identifier.
      /// </summary>
      public string DocumentId { get; internal set; }
   }
}