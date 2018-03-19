namespace IndexTankDotNet.Tests
{
   using System;
   using System.Collections.Generic;
   using System.Threading;
   using Microsoft.VisualStudio.TestTools.UnitTesting;

   /// <summary>
   /// These tests are designed to run against a real IndexTank Account.
   /// You must supply the private URL of an existing IndexTank account to run these tests.
   /// WARNING: RUNNING THESE TESTS will DELETE all existing indexes in the account.
   /// </summary>
   [TestClass]
   public class CategoriesTests
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
      public void Can_Add_Categories_To_Existing_Document_Without_Categories()
      {
         Document document = new Document("item_1").AddField("text", "This is the item description");

         testIndex.AddDocument(document);

         var categories = new Dictionary<string, string>();
         categories.Add("articleType", "camera");
         categories.Add("priceRange", "$0 to $299");

         Assert.IsTrue(testIndex.UpdateCategories(document.DocumentId, categories));
      }

      [TestMethod]
      public void Can_Update_Categories_On_Existing_Document_With_Categories()
      {
         Document document = new Document("item_2").AddField("text", "This is the item description")
            .AddCategory("articleType", "game console")
            .AddCategory("priceRange", "$300 to $599");

         testIndex.AddDocument(document);

         var categories = new Dictionary<string, string>();
         categories.Add("articleType", "camera");
         categories.Add("priceRange", "$0 to $299");

         Assert.IsTrue(testIndex.UpdateCategories(document.DocumentId, categories));
      }

      [TestMethod]
      public void Can_Update_Single_Category_On_Existing_Document()
      {
         Document document = new Document("item_2").AddField("text", "This is the item description")
            .AddCategory("articleType", "game console")
            .AddCategory("priceRange", "$300 to $599");

         testIndex.AddDocument(document);
        
         Assert.IsTrue(testIndex.UpdateCategory(document.DocumentId, "articleType", "camera"));
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Update_Single_Category_With_Null_DocId()
      {
         testIndex.UpdateCategory(null, "articleType", "camera");
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Update_Single_Category_With_Empty_DocId()
      {
         testIndex.UpdateCategory(string.Empty, "articleType", "camera");
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Update_Single_Category_With_Null_Category_Name()
      {
         Document document = new Document("item_2").AddField("text", "This is the item description")
            .AddCategory("articleType", "game console")
            .AddCategory("priceRange", "$300 to $599");

         testIndex.AddDocument(document);
         
         testIndex.UpdateCategory("item_2", null, "camera");
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Update_Single_Category_With_Empty_Category_Name()
      {
         Document document = new Document("item_2").AddField("text", "This is the item description")
            .AddCategory("articleType", "game console")
            .AddCategory("priceRange", "$300 to $599");

         testIndex.AddDocument(document);

         testIndex.UpdateCategory("item_2", string.Empty, "camera");
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Update_Single_Category_With_Null_Category_Value()
      {
         Document document = new Document("item_2").AddField("text", "This is the item description")
            .AddCategory("articleType", "game console")
            .AddCategory("priceRange", "$300 to $599");

         testIndex.AddDocument(document);

         testIndex.UpdateCategory("item_2", "articleType", null);
      }

      [TestMethod]
      public void Can_Update_Single_Category_With_Empty_Category_Value()
      {
         Document document = new Document("item_2").AddField("text", "This is the item description")
            .AddCategory("articleType", "game console")
            .AddCategory("priceRange", "$300 to $599");

         testIndex.AddDocument(document);

         Assert.IsTrue(testIndex.UpdateCategory("item_2", "articleType", string.Empty));
      }

      [TestMethod]
      public void Can_Update_Categories_With_Empty_Category_Value()
      {
         Document document = new Document("item_7").AddField("text", "This is the item description");

         testIndex.AddDocument(document);

         var categories = new Dictionary<string, string>();
         categories.Add("articleType", "");
         categories.Add("priceRange", "$0 to $299");

         Assert.IsTrue(testIndex.UpdateCategories(document.DocumentId, categories));
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Update_Categories_With_Empty_Categories_List()
      {
         Document document = new Document("item_6").AddField("text", "This is the item description");

         testIndex.AddDocument(document);

         var categories = new Dictionary<string, string>();

         testIndex.UpdateCategories(document.DocumentId, categories);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Update_Categories_With_Null_Categories_List()
      {
         Document document = new Document("item_5").AddField("text", "This is the item description");

         testIndex.AddDocument(document);

         testIndex.UpdateCategories(document.DocumentId, null);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Update_Categories_With_Null_DocId()
      {
         Document document = new Document("item_3").AddField("text", "This is the item description");

         testIndex.AddDocument(document);

         var categories = new Dictionary<string, string>();
         categories.Add("articleType", "camera");
         categories.Add("priceRange", "$0 to $299");

         testIndex.UpdateCategories(null, categories);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Update_Categories_With_Empty_DocId()
      {
         Document document = new Document("item_4").AddField("text", "This is the item description");

         testIndex.AddDocument(document);

         var categories = new Dictionary<string, string>();
         categories.Add("articleType", "camera");
         categories.Add("priceRange", "$0 to $299");

         testIndex.UpdateCategories(string.Empty, categories);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Update_Categories_With_Invalid_Categories_1()
      {
         Document document = new Document("item_8").AddField("text", "This is the item description");

         testIndex.AddDocument(document);

         var categories = new Dictionary<string, string>();
         categories.Add("", "camera");
         categories.Add("priceRange", "$0 to $299");

         testIndex.UpdateCategories(document.DocumentId, categories);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Update_Categories_With_Invalid_Categories_2()
      {
         Document document = new Document("item_9").AddField("text", "This is the item description");

         testIndex.AddDocument(document);

         var categories = new Dictionary<string, string>();
         categories.Add("articleType", null);
         categories.Add("priceRange", "$0 to $299");

         testIndex.UpdateCategories(document.DocumentId, categories);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Update_Categories_With_Invalid_Categories_3()
      {
         Document document = new Document("item_10").AddField("text", "This is the item description");

         testIndex.AddDocument(document);

         var categories = new Dictionary<string, string>();
         categories.Add(null, "camera");
         categories.Add("priceRange", "$0 to $299");

         testIndex.UpdateCategories(document.DocumentId, categories);
      }
   }
}