namespace IndexTankDotNet.Tests
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Net;
   using Microsoft.VisualStudio.TestTools.UnitTesting;

   /// <summary>
   /// These tests are designed to run against a real IndexTank Account.
   /// You must supply the private URL of an existing IndexTank account to run these tests.
   /// WARNING: RUNNING THESE TESTS will DELETE all existing indexes in the account.
   /// </summary>
   [TestClass]
   public class IndexTests
   {
      private static IndexTankClient indexTankClient;

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
      }

      [TestInitialize]
      public void TestInitialize()
      {
         DeleteAllIndexes();
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
      public void Can_Create_Index()
      {
         Index index = indexTankClient.CreateIndex("test_index");

         Assert.AreEqual("test_index", index.Name);
         Assert.AreEqual(0, index.Size);

         // public search and suggestions are disabled by default
         Assert.IsFalse(index.IsPublicApiEnabled);
         Assert.IsFalse(index.AreSuggestionsEnabled);
      }      

      [TestMethod]
      public void Can_Create_Index_With_Public_Search_Enabled()
      {
         Index index = indexTankClient.CreateIndex("test_index", true);

         Assert.AreEqual("test_index", index.Name);
         Assert.AreEqual(0, index.Size);
         Assert.IsTrue(index.IsPublicApiEnabled);
      }

      [TestMethod]
      [ExpectedException(typeof(NotSupportedException))]
      public void Can_Create_Index_With_Suggestions_Enabled()
      {
         Index index = indexTankClient.CreateIndex("test_index", false, true);

         Assert.AreEqual("test_index", index.Name);
         Assert.AreEqual(0, index.Size);
         Assert.IsTrue(index.AreSuggestionsEnabled);
      }

      [TestMethod]
      public void Create_Index_That_Already_Exists_Returns_Null()
      {
         indexTankClient.CreateIndex("test_index");
         Index index = indexTankClient.CreateIndex("test_index");

         Assert.IsNull(index);
      }

      [TestMethod]
      public void Can_Get_Index()
      {
         Index index = indexTankClient.CreateIndex("test_index");
         Index retrievedIndex = indexTankClient.GetIndex("test_index");

         Assert.AreEqual("test_index", retrievedIndex.Name);
         Assert.AreEqual(0, index.Size);
         Assert.IsFalse(retrievedIndex.IsPublicApiEnabled);
      }

      [TestMethod]
      public void Can_Get_All_Indexes()
      {
         indexTankClient.CreateIndex("test_index_1");
         indexTankClient.CreateIndex("test_index_2");
         indexTankClient.CreateIndex("test_index_3");
         indexTankClient.CreateIndex("test_index_4");

         List<Index> indexes = indexTankClient.GetIndexes().ToList();

         Assert.IsFalse(indexes.FindIndex(i => i.Name == "test_index_1") == -1);
         Assert.IsFalse(indexes.FindIndex(i => i.Name == "test_index_2") == -1);
         Assert.IsFalse(indexes.FindIndex(i => i.Name == "test_index_3") == -1);
         Assert.IsFalse(indexes.FindIndex(i => i.Name == "test_index_4") == -1);
      }

      [TestMethod]
      public void Can_Update_Public_Search_On_Existing_Index()
      {
         indexTankClient.CreateIndex("test_index");

         Index retrievedIndex = indexTankClient.GetIndex("test_index");

         bool result = retrievedIndex.UpdateIndex(!retrievedIndex.IsPublicApiEnabled, retrievedIndex.AreSuggestionsEnabled);

         Index updatedIndex = indexTankClient.GetIndex("test_index");

         Assert.IsTrue(result);
         Assert.IsTrue(retrievedIndex.IsPublicApiEnabled != updatedIndex.IsPublicApiEnabled);
      }

      [TestMethod]
      [ExpectedException(typeof(NotSupportedException))]
      public void Can_Update_Suggestions_On_Existing_Index()
      {
         indexTankClient.CreateIndex("test_index");

         Index retrievedIndex = indexTankClient.GetIndex("test_index");

         bool result = retrievedIndex.UpdateIndex(retrievedIndex.IsPublicApiEnabled, !retrievedIndex.AreSuggestionsEnabled);

         Index updatedIndex = indexTankClient.GetIndex("test_index");

         Assert.IsTrue(result);
         Assert.IsTrue(retrievedIndex.AreSuggestionsEnabled != updatedIndex.AreSuggestionsEnabled);
      }

      [TestMethod]
      public void Can_Delete_Existing_Index()
      {
         IndexTankApiException exception = null;

         indexTankClient.CreateIndex("test_index");

         indexTankClient.DeleteIndex("test_index");

         try
         {
            indexTankClient.GetIndex("test_index");
         }
         catch (IndexTankApiException ex)
         {
            exception = ex;
         }

         Assert.IsNotNull(exception);
         Assert.AreEqual(HttpStatusCode.NotFound, exception.GetHttpStatusCode());
      }

      [TestMethod]
      public void Can_Delete_Non_Existent_Index()
      {
         Assert.IsTrue(indexTankClient.DeleteIndex("no_such_index"));
      }

      [TestMethod]
      [ExpectedException(typeof(FormatException))]
      public void Cannot_Create_Index_With_Illegal_Characters_In_Name()
      {
         indexTankClient.CreateIndex("thisis@bad");
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Create_Index_With_Null_Name()
      {
         indexTankClient.CreateIndex(null);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Create_Index_With_Empty_Name()
      {
         indexTankClient.CreateIndex(string.Empty);
      }

      [TestMethod]
      [ExpectedException(typeof(FormatException))]
      public void Cannot_Create_Index_With_Forward_Slash_In_Name()
      {
         indexTankClient.CreateIndex("test/_index");
      }

      [TestMethod]
      public void Cannot_Retrieve_Non_Existent_Index()
      {
         IndexTankApiException exception = null;

         try
         {
            indexTankClient.GetIndex("no_such_index");
         }
         catch (IndexTankApiException ex)
         {
            exception = ex;
         }

         Assert.IsNotNull(exception);
         Assert.AreEqual(HttpStatusCode.NotFound, exception.GetHttpStatusCode());
      }

      [TestMethod]
      public void Cannot_Create_Too_Many_Indexes()
      {
         // The maximum number of indices allowed under a free account is 5.

         IndexTankApiException exception = null;
         int counter = 0;

         do
         {
            try
            {
               indexTankClient.CreateIndex("test_index_" + counter);
               counter++;
            }
            catch (IndexTankApiException ex)
            {
               exception = ex;
            }

         } while (exception == null);

         Assert.IsNotNull(exception);
         Assert.IsTrue(exception.GetHttpStatusCode() == HttpStatusCode.Conflict);
         Assert.AreEqual("There are too many indexes for this account.", exception.Message);
      }      
   }
}