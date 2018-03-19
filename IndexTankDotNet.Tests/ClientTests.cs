namespace IndexTankDotNet.Tests
{
   using System;
   using Microsoft.VisualStudio.TestTools.UnitTesting;

   /// <summary>
   /// These tests are designed to run against a real IndexTank Account.
   /// You must supply the private URL of an existing IndexTank account to run these tests.
   /// WARNING: RUNNING THESE TESTS will DELETE all existing indexes in the account.
   /// </summary>
   [TestClass]
   public class ClientTests
   {
      [TestMethod]
      public void Can_Create_Client_With_Uri()
      {
         Uri privateUri = new Uri(TestResources.PRIVATE_URL);

         var client = new IndexTankClient(privateUri);

         Assert.IsNotNull(client);         
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Create_Client_With_Null_Url_String()
      {
         var client = new IndexTankClient((string) null);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Create_Client_With_Null_Uri()
      {
         var client = new IndexTankClient((Uri)null);
      }

      [TestMethod]
      [ExpectedException(typeof(UriFormatException))]
      public void Cannot_Create_Client_With_Empty_Url_String()
      {
         var client = new IndexTankClient(string.Empty);
      }

      [TestMethod]
      [ExpectedException(typeof(UriFormatException))]
      public void Cannot_Create_Client_With_Invalid_Url_String()
      {
         var client = new IndexTankClient("fjuiso0s87k");
      }
   }
}