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
   public class VariablesTests
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
      public void Can_Add_Variables_To_Existing_Document()
      {
         Document document = new Document("post_1").AddField("text", "I love Bioshock");

         testIndex.AddDocument(document);

         var variables = new Dictionary<int, float>();
         variables.Add(2, 0.7f);
         variables.Add(1, 0.6f);
         variables.Add(0, 0.5f);

         Assert.IsTrue(testIndex.UpdateVariables(document.DocumentId, variables));
      }      

      [TestMethod]
      public void Can_Update_Variables_On_Existing_Document()
      {
         Document document = new Document("post_2").AddField("text", "I love Tetris")
            .AddVariable(0, 0.1f)
            .AddVariable(1, 0.2f)
            .AddVariable(2, 0.3f);

         testIndex.AddDocument(document);

         var variables = new Dictionary<int, float>();
         variables.Add(0, 0.5f);
         variables.Add(1, 0.6f);
         variables.Add(2, 0.7f);

         Assert.IsTrue(testIndex.UpdateVariables(document.DocumentId, variables));
      }

      [TestMethod]
      public void Can_Update_Single_Variable_On_Existing_Document()
      {
         Document document = new Document("post_2").AddField("text", "I love Tetris")
            .AddVariable(0, 0.1f)
            .AddVariable(1, 0.2f)
            .AddVariable(2, 0.3f);

         testIndex.AddDocument(document);

         Assert.IsTrue(testIndex.UpdateVariable(document.DocumentId, 0, 0.5f));
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Update_Single_Variable_With_Null_DocId()
      {
         testIndex.UpdateVariable(null, 0, 0.5f);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Update_Single_Variable_With_Empty_DocId()
      {
         testIndex.UpdateVariable(string.Empty, 0, 0.5f);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Update_Variables_With_Null_DocId()
      {
         Document document = new Document("post_3").AddField("text", "I love Bioshock");

         testIndex.AddDocument(document);

         var variables = new Dictionary<int, float>();

         variables.Add(0, 0.5f);
         variables.Add(1, 0.6f);
         variables.Add(2, 0.7f);

         testIndex.UpdateVariables(null, variables);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Update_Variables_With_Empty_DocId()
      {
         Document document = new Document("post_4").AddField("text", "I love Bioshock");

         testIndex.AddDocument(document);

         var variables = new Dictionary<int, float>();

         variables.Add(0, 0.5f);
         variables.Add(1, 0.6f);
         variables.Add(2, 0.7f);

         testIndex.UpdateVariables(string.Empty, variables);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Update_Variables_With_Null_Variables()
      {
         Document document = new Document("post_5").AddField("text", "I love Bioshock");

         testIndex.AddDocument(document);

         testIndex.UpdateVariables(document.DocumentId, null);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Update_Variables_With_Empty_Variables_List()
      {
         Document document = new Document("post_6").AddField("text", "I love Bioshock");

         testIndex.AddDocument(document);

         var variables = new Dictionary<int, float>();

         testIndex.UpdateVariables(document.DocumentId, variables);
      }

      [TestMethod]
      public void Cannot_Update_Variables_With_Invalid_Variable()
      {
         Document document = new Document("post_7").AddField("text", "I love Bioshock");

         testIndex.AddDocument(document);

         var variables = new Dictionary<int, float>();
         variables.Add(3, 10.0f);

         IndexTankApiException exception = null;

         try
         {
            testIndex.UpdateVariables(document.DocumentId, variables);
         }
         catch (IndexTankApiException ex)
         {
            exception = ex;
         }

         Assert.IsNotNull(exception);
         Assert.AreEqual("Invalid argument. Make sure you have not specified a variable number that exceeds the number that is allowed for the current index.",
                         exception.Message);
      }
   }
}