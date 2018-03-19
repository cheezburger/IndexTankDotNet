namespace IndexTankDotNet
{
   using System.Collections.Generic;
   using System.Linq;

   /// <summary>
   /// Represents a collection of BatchDeleteResult objects that can interated over, or used to resubmit deletions that failed.
   /// </summary>
   public class BatchDeleteResultCollection : List<BatchDeleteResult>
   {
      /// <summary>
      /// Gets an array of strings containing the ids of the documents for which batch deletion failed.
      /// </summary>
      /// <returns>Returns a string[].</returns>
      public string[] GetFailedDocIds()
      {
         return this.Where(r => !string.IsNullOrWhiteSpace(r.Error)).Select(r => r.DocumentId).ToArray();
      }
   }
}