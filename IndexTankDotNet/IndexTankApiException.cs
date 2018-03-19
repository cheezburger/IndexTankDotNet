namespace IndexTankDotNet
{
   using System;
   using System.Net;
   using System.Runtime.Serialization;

   /// <summary>
   /// The exception that is thrown to represent errors returned from the IndexTank API.
   /// </summary>
   [Serializable]
   public class IndexTankApiException : IndexTankException
   {
      internal IndexTankApiException(string message, HttpStatusCode statusCode) : base(message)
      {
         exceptionState.HttpStatusCode = statusCode;
      }      
   }
}