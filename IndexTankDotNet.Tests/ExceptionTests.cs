namespace IndexTankDotNet.Tests
{
   using System;
   using System.Collections.Generic;
   using System.IO;
   using System.Net;
   using System.Runtime.Serialization.Formatters.Binary;
   using System.Threading;
   using Microsoft.VisualStudio.TestTools.UnitTesting;

   [TestClass]
   public class ExceptionTests
   {
      private static IndexTankClient indexTankClient;
      private static Index testIndex;

      [ClassInitialize]
      public static void ClassInitialize(TestContext textContext)
      {
         if (string.IsNullOrWhiteSpace(TestResources.PRIVATE_URL))
         {
            throw new InvalidOperationException(
               "No URL was provided. You must supply the private URL for your IndexTank account to run these tests.");
         }

         indexTankClient = new IndexTankClient(TestResources.PRIVATE_URL);

         DeleteAllIndexes();

         testIndex = indexTankClient.CreateIndex("test_index");

         while (!testIndex.IsStarted)
         {
            Thread.Sleep(300);
            testIndex = indexTankClient.GetIndex("test_index");
         }
      }

      [ClassCleanup]
      public static void ClassCleanup()
      {
         DeleteAllIndexes();
      }

      private static void DeleteAllIndexes()
      {
         IEnumerable<Index> testIndexes = indexTankClient.GetIndexes();

         foreach (Index index in testIndexes)
         {
            indexTankClient.DeleteIndex(index.Name);
         }
      }

      [TestMethod]
      public void Can_Serialize_IndexTankApiException()
      {
         try
         {
            Index index = indexTankClient.GetIndex("no_such_index");
            index.DeleteDocument("post_1");
         }
         catch (IndexTankApiException ex)
         {
            IndexTankApiException deserialized;
            using (var fileStream = new FileStream("IndexTankException.dat", FileMode.Create))
            {
               var formatter = new BinaryFormatter();
               formatter.Serialize(fileStream, ex);

               fileStream.Position = 0;
               deserialized = (IndexTankApiException) formatter.Deserialize(fileStream);
            }

            Assert.AreEqual(HttpStatusCode.NotFound, deserialized.GetHttpStatusCode());
         }
      }

      [TestMethod]
      public void Can_Serialize_IndexTankProtocolException()
      {
         try
         {
            indexTankClient = new IndexTankClient("http://:11aaaAaAa1a+aA@a11aa.api.indextank.com");
            indexTankClient.GetIndexes();
         }
         catch (IndexTankProtocolException ex)
         {
            IndexTankProtocolException deserialized;
            using (var fileStream = new FileStream("IndexTankException.dat", FileMode.Create))
            {
               var formatter = new BinaryFormatter();
               formatter.Serialize(fileStream, ex);

               fileStream.Position = 0;
               deserialized = (IndexTankProtocolException) formatter.Deserialize(fileStream);
            }

            Assert.AreEqual(HttpStatusCode.OK, deserialized.GetHttpStatusCode());
         }
      }
   }
}