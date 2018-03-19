namespace IndexTankDotNet.Tests
{
   using System.Net;
   using Microsoft.VisualStudio.TestTools.UnitTesting;

   [TestClass]
   public class ProtocolTests
   {
      private IndexTankClient indexTankClient;

      [TestMethod]
      [ExpectedException(typeof(IndexTankProtocolException))]
      public void Cannot_Get_Indexes_With_Wrong_Private_Url()
      {
         indexTankClient = new IndexTankClient("http://:11aaaAaAa1a+aA@a11aa.api.indextank.com");
         indexTankClient.GetIndexes();
      }

      [TestMethod]
      [ExpectedException(typeof(IndexTankProtocolException))]
      public void Cannot_Get_Index_With_Wrong_Private_Url()
      {
         indexTankClient = new IndexTankClient("http://:11aaaAaAa1a+aA@a11aa.api.indextank.com");
         indexTankClient.GetIndex("test_index");
      }

      [TestMethod]
      [ExpectedException(typeof(IndexTankProtocolException))]
      public void Cannot_Create_Index_With_Wrong_Private_Url()
      {
         indexTankClient = new IndexTankClient("http://:11aaaAaAa1a+aA@a11aa.api.indextank.com");
         indexTankClient.CreateIndex("test_index");
      }

      [TestMethod]
      public void Can_Delete_Index_With_Wrong_PrivateUrl()
      {
         indexTankClient = new IndexTankClient("http://:11aaaAaAa1a+aA@a11aa.api.indextank.com");

         Assert.IsTrue(indexTankClient.DeleteIndex("test_index"));
      }

      [TestMethod]
      [ExpectedException(typeof(IndexTankProtocolException))]
      public void Cannot_Get_Indexes_With_Unresolvable_Private_Url()
      {
         indexTankClient = new IndexTankClient("http://:11aaaAaAa1a+aA@a11aa.api.indextan.com");
         indexTankClient.GetIndexes();
      }

      [TestMethod]
      [ExpectedException(typeof(IndexTankProtocolException))]
      public void Cannot_Get_Index_With_Unresolvable_Private_Url()
      {
         indexTankClient = new IndexTankClient("http://:11aaaAaAa1a+aA@a11aa.api.indextan.com");
         indexTankClient.GetIndex("test_index");
      }

      [TestMethod]
      [ExpectedException(typeof(IndexTankProtocolException))]
      public void Cannot_Create_Index_With_Unresolvable_Private_Url()
      {
         indexTankClient = new IndexTankClient("http://:11aaaAaAa1a+aA@a11aa.api.indextan.com");
         indexTankClient.CreateIndex("test_index");
      }

      [TestMethod]
      [ExpectedException(typeof(IndexTankProtocolException))]
      public void Cannot_Delete_Index_With_Unresolvable_Private_Url()
      {
         indexTankClient = new IndexTankClient("http://:11aaaAaAa1a+aA@a11aa.api.indextan.com");

         indexTankClient.DeleteIndex("test_index");
      }

      [TestMethod]
      public void Cannot_Get_Indexes_With_Bad_Password()
      {
         indexTankClient = new IndexTankClient("http://:11aaaAaAa1a+aA@d17ka.api.indextank.com");

         IndexTankProtocolException exception = null;

         try
         {
            indexTankClient.GetIndexes();
         }
         catch (IndexTankProtocolException ex)
         {
            exception = ex;
         }

         Assert.IsNotNull(exception);
         Assert.AreEqual(HttpStatusCode.Unauthorized, exception.GetHttpStatusCode());
      }

      [TestMethod]
      public void Cannot_Get_Index_With_Bad_Password()
      {
         indexTankClient = new IndexTankClient("http://:11aaaAaAa1a+aA@d17ka.api.indextank.com");

         IndexTankProtocolException exception = null;

         try
         {
            indexTankClient.GetIndex("test_index");
         }
         catch (IndexTankProtocolException ex)
         {
            exception = ex;
         }

         Assert.IsNotNull(exception);
         Assert.AreEqual(HttpStatusCode.Unauthorized, exception.GetHttpStatusCode());
      }

      [TestMethod]
      public void Cannot_Create_Index_With_Bad_Password()
      {
         indexTankClient = new IndexTankClient("http://:11aaaAaAa1a+aA@d17ka.api.indextank.com");

         IndexTankProtocolException exception = null;

         try
         {
            indexTankClient.CreateIndex("test_index");
         }
         catch (IndexTankProtocolException ex)
         {
            exception = ex;
         }

         Assert.IsNotNull(exception);
         Assert.AreEqual(HttpStatusCode.Unauthorized, exception.GetHttpStatusCode());
      }

      [TestMethod]
      public void Cannot_Delete_Index_With_Bad_Password()
      {
         indexTankClient = new IndexTankClient("http://:11aaaAaAa1a+aA@d17ka.api.indextank.com");

         IndexTankProtocolException exception = null;

         try
         {
            indexTankClient.DeleteIndex("test_index");
         }
         catch (IndexTankProtocolException ex)
         {
            exception = ex;
         }

         Assert.IsNotNull(exception);
         Assert.AreEqual(HttpStatusCode.Unauthorized, exception.GetHttpStatusCode());
      }
   }
}