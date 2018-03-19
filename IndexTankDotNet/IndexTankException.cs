namespace IndexTankDotNet
{
   using System;
   using System.Net;
   using System.Runtime.Serialization;
   using System.Security;

   /// <summary>
   /// The base exception that represents both 1) errors returned from the IndexTank API, and 2) errors caused by connection failures, failed DNS lookups, invalid credentials, and the like.
   /// </summary>
   [Serializable]
   public abstract class IndexTankException : Exception
   {
      protected IndexTankExceptionState exceptionState;

      protected internal IndexTankException(string message)
         : this(message, null)
      {
      }

      protected internal IndexTankException(string message, Exception innerException)
         : base(message, innerException)
      {
         SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(exceptionState);
      }      

      /// <summary>
      /// Gets the HTTP status code associated with an error.
      /// </summary>
      /// <returns>Returns an HttpStatusCode.</returns>
      public HttpStatusCode GetHttpStatusCode()
      {
         return exceptionState.HttpStatusCode;
      }      

      [Serializable]
      protected struct IndexTankExceptionState : ISafeSerializationData
      {
         public HttpStatusCode HttpStatusCode { get; set; }

         public void CompleteDeserialization(object deserialized)
         {
            IndexTankException exception = (IndexTankException) deserialized;
            exception.exceptionState = this;
         }
      }
   }
}