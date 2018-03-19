namespace IndexTankDotNet
{
   using System.Collections.Generic;
   using System.Linq;

   /// <summary>
   /// Represents a collection of BatchIndexResult objects that can interated over, or used to resubmit document additions that failed.
   /// </summary>
   public class BatchIndexResultCollection : List<BatchIndexResult>
   {
      /// <summary>
      /// A list of Document objects containing the documents for which batch addition failed.
      /// </summary>
      /// <returns>Returns an IList&lt;Document&gt;.</returns>
      public IList<Document> GetFailedDocuments()
      {
         return this.Where(r => !string.IsNullOrWhiteSpace(r.Error)).Select(r => r.Document).ToList();
      }
   }
}