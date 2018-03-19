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
   public class FunctionTests
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
      public void Can_Add_Function()
      {
         Assert.IsTrue(testIndex.AddFunction(1, "relevance"));
         Assert.IsTrue(testIndex.AddFunction(2, "log(boost(0)) - age/50000")); //mytodo what is boost?
      }

      [TestMethod]
      public void Can_Overwrite_Existing_Function()
      {
         Assert.AreEqual("log(boost(0)) - age/50000", testIndex.GetFunctions()[2]);
         Assert.IsTrue(testIndex.AddFunction(2, "pow(doc.var[0], 3) * doc.var[1]"));
         Assert.AreEqual("pow(doc.var[0], 3) * doc.var[1]", testIndex.GetFunctions()[2]);
      }

      [TestMethod]
      public void Can_Get_Functions()
      {
         testIndex.AddFunction(1, "relevance");
         testIndex.AddFunction(2, "log(boost(0)) - age/50000");

         IDictionary<int, string> functions = testIndex.GetFunctions();

         Assert.AreEqual("-age", functions[0]);
         Assert.AreEqual("relevance", functions[1]);
         Assert.AreEqual("log(boost(0)) - age/50000", functions[2]);
      }

      [TestMethod]
      public void Can_Delete_Function()
      {
         testIndex.AddFunction(1, "relevance");

         IDictionary<int, string> functions = testIndex.GetFunctions();

         Assert.IsTrue(testIndex.GetFunctions().ContainsKey(1));
         Assert.AreEqual("relevance", functions[1]);
         Assert.IsTrue(testIndex.DeleteFunction(1));
         Assert.IsFalse(testIndex.GetFunctions().ContainsKey(1));
      }

      [TestMethod]
      public void Can_Delete_Non_Existent_Function()
      {
         Assert.IsTrue(testIndex.DeleteFunction(50));
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void Cannot_Delete_With_Negative_Function_Num()
      {
         Assert.IsTrue(testIndex.DeleteFunction(-4));
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void Cannot_Add_Function_With_Negative_Function_Num()
      {
         testIndex.AddFunction(-1, "log(doc.var[0]) - age/86400");
      }

      [TestMethod]
      public void Cannot_Add_Function_With_Malformed_Definition()
      {
         IndexTankApiException exception = null;

         try
         {
            testIndex.AddFunction(3, "foo(doc.var[0]) - age/86400");
         }
         catch (IndexTankApiException ex)
         {
            exception = ex;
         }

         Assert.IsNotNull(exception);
         Assert.AreEqual("The function definition is malformed. Check your syntax.", exception.Message);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Add_Function_With_Null_Definition()
      {
         testIndex.AddFunction(3, null);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Add_Function_With_Empty_Definition()
      {
         testIndex.AddFunction(3, string.Empty);
      }
   }
}