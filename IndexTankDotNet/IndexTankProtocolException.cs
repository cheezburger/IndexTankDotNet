namespace IndexTankDotNet
{
   using System;
   using System.Net;
   using System.Runtime.Serialization;

   /// <summary>
   /// The exception that is thrown to represent connection failures, failed DNS lookups, invalid credentials, and the like.
   /// </summary>
   [Serializable]
   public class IndexTankProtocolException : IndexTankException
   {
      internal IndexTankProtocolException(string message, Exception innerException, HttpStatusCode statusCode)
         : base(message, innerException)
      {
         exceptionState.HttpStatusCode = statusCode;
      }      
   }
}