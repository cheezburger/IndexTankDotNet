namespace IndexTankDotNet.Tests
{
   using System;
   using System.Collections.Generic;
   using System.Net;
   using System.Text;
   using System.Threading;
   using Microsoft.VisualStudio.TestTools.UnitTesting;

   /// <summary>
   /// These tests are designed to run against a real IndexTank Account.
   /// You must supply the private URL of an existing IndexTank account to run these tests.
   /// WARNING: RUNNING THESE TESTS will DELETE all existing indexes in the account.
   /// </summary>
   [TestClass]
   public class DocumentTests
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
      public void Can_Add_Document_With_Text_Field()
      {
         var document = new Document("post_1", "I love Bioshock");

         Assert.IsTrue(testIndex.AddDocument(document));
      }

      [TestMethod]
      public void Can_Add_Document_With_Space_In_DocId()
      {
         Document document = new Document("this is post 1").AddField("text", "I love Bioshock");

         Assert.IsTrue(testIndex.AddDocument(document));
      }

      [TestMethod]
      public void Can_Add_Document_With_Other_Field()
      {
         Document document = new Document("post_2").AddField("title", "Post 2 Title");

         Assert.IsTrue(testIndex.AddDocument(document));
      }

      [TestMethod]
      public void Can_Add_Document_With_Overwritten_Field()
      {
         Document document = new Document("post_21").AddField("title", "Post 2 Title").AddField("title", "Title of Post 2");

         Assert.IsTrue(testIndex.AddDocument(document));
      }

      [TestMethod]
      public void Can_Add_Document_With_Timestamp()
      {
         Document document = new Document("post_22").AddTimestamp(DateTime.Now);

         Assert.IsTrue(testIndex.AddDocument(document));
      }

      [TestMethod]
      public void Can_Add_Document_With_Timestamp_Before_Unix_Epoch()
      {
         Document document = new Document("post_23", "test document").AddTimestamp(new DateTime(1969, 7, 4));

         Assert.IsTrue(testIndex.AddDocument(document));
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void Cannot_Add_Document_With_Timestamp_Too_Far_In_Future()
      {
         Document document = new Document("post_22").AddTimestamp(DateTime.MaxValue);

         Assert.IsFalse(testIndex.AddDocument(document));
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void Cannot_Add_Document_With_Timestamp_Too_Large()
      {
         Document document = new Document("post_24").AddField("timestamp", ((long) int.MaxValue + 1).ToString());

         Assert.IsFalse(testIndex.AddDocument(document));
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void Cannot_Add_Document_With_Timestamp_Too_Small()
      {
         Document document = new Document("post_25").AddField("timestamp", ((long)int.MinValue - 1).ToString());

         Assert.IsFalse(testIndex.AddDocument(document));
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Add_Document_With_Empty_Timestamp()
      {
         Document document = new Document("post_25").AddField("timestamp", string.Empty);

         Assert.IsFalse(testIndex.AddDocument(document));
      }

      [TestMethod]
      [ExpectedException(typeof(FormatException))]
      public void Cannot_Add_Document_With_NonInteger_Timestamp()
      {
         Document document = new Document("post_25").AddField("timestamp", "fizzbin");

         Assert.IsFalse(testIndex.AddDocument(document));
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void Cannot_Add_Document_With_Timestamp_Too_Far_In_Past()
      {
         new Document("post_22").AddTimestamp(DateTime.MinValue);
      }

      [TestMethod]
      public void Can_Add_Document_With_Empty_Field_Value()
      {
         new Document("post_14").AddField("title", string.Empty);
      }

      [TestMethod]
      public void Can_Add_Document_With_Url_Based_Id()
      {
         Document document = new Document("games/reviews/1024").AddField("title",
                                                                         "The id for this post has forward slashes");

         Assert.IsTrue(testIndex.AddDocument(document));
      }

      [TestMethod]
      public void Can_Add_Document_With_Variables()
      {
         Document document = new Document("post_3").AddField("text", "I love Tetris").AddVariable(0, 0.5f);

         Assert.IsTrue(testIndex.AddDocument(document));
      }

      [TestMethod]
      public void Can_Add_Document_With_Categories()
      {
         Document document = new Document("post_9").AddField("text", "I love Super Mario Bros").AddCategory("post type",
                                                                                                            "game review");

         Assert.IsTrue(testIndex.AddDocument(document));
      }

      [TestMethod]
      public void Can_Add_Document_With_Empty_Valued_Category()
      {
         Document document = new Document("post_4").AddField("text", "I love Angry Birds").AddCategory("post type", string.Empty);

         Assert.IsTrue(testIndex.AddDocument(document));
      }

      [TestMethod]
      public void Can_Add_Multiple_Documents()
      {
         var documents = new List<Document>
                            {
                               new Document("post_15").AddField("text", "I love Call of Duty"),
                               new Document("post_16").AddField("text", "I love Asteroids"),
                               new Document("post_17").AddField("text", "I love Crysis")
                            };

         BatchIndexResultCollection results = testIndex.AddDocuments(documents);

         Assert.AreEqual(3, results.Count);
         Assert.AreEqual(0, results.GetFailedDocuments().Count);
         Assert.AreEqual("post_15", results[0].Document.DocumentId);
         Assert.AreEqual("post_16", results[1].Document.DocumentId);
         Assert.AreEqual("post_17", results[2].Document.DocumentId);
      }

      [TestMethod]
      public void Can_Resubmit_Failed_Additions()
      {
         var documents = new List<Document>
                            {
                               new Document("post_15").AddField("text", "I love Call of Duty"),
                               new Document("post_16").AddField("text", "I love Asteroids"),
                               new Document("post_17").AddField("text", "I love Crysis")
                            };

         BatchIndexResultCollection results = testIndex.AddDocuments(documents);

         IList<Document> failedDocuments = results.GetFailedDocuments();

         if (failedDocuments.Count > 0)
         {
            testIndex.AddDocuments(failedDocuments);
         }
      }

      [TestMethod]
      public void Can_Delete_Document()
      {
         Document document = new Document("post_13").AddField("text", "I love Sonic the Hedgehog");

         testIndex.AddDocument(document);

         Assert.IsTrue(testIndex.DeleteDocument(document.DocumentId));
      }

      [TestMethod]
      public void Can_Delete_Document_With_Url_Based_DocId()
      {
         Document document = new Document("games/reviews/1024").AddField("text", "I love Sonic the Hedgehog");

         testIndex.AddDocument(document);

         Assert.IsTrue(testIndex.DeleteDocument("games/reviews/1024"));
      }

      [TestMethod]
      public void Can_Delete_Document_With_Space_In_DocId()
      {
         Document document = new Document("games reviews 1024").AddField("text", "I love Sonic the Hedgehog");

         testIndex.AddDocument(document);
         Assert.IsTrue(testIndex.DeleteDocument("games reviews 1024"));
      }

      [TestMethod]
      public void Can_Delete_Non_Existent_Document()
      {
         Assert.IsTrue(testIndex.DeleteDocument("no_such_document"));
      }

      [TestMethod]
      public void Can_Delete_Multiple_Documents()
      {
         var documents = new List<Document>
                            {
                               new Document("post/18").AddField("text", "Review of Call of Duty"),
                               new Document("post/19").AddField("text", "Review of Asteroids"),
                               new Document("post/20").AddField("text", "Review of Crysis")
                            };

         testIndex.AddDocuments(documents);

         var docIds = new[] {"post/18", "post/19", "post/20"};

         SearchResult searchResult1 = testIndex.Search("review");

         BatchDeleteResultCollection deleteResults = testIndex.DeleteDocuments(docIds);

         SearchResult searchResult2 = testIndex.Search("review");

         Assert.AreEqual(3, searchResult1.Matches);
         Assert.AreEqual(0, searchResult2.Matches);
         Assert.AreEqual(3, deleteResults.Count);
         Assert.AreEqual(0, deleteResults.GetFailedDocIds().Length);
         Assert.AreEqual("post/18", deleteResults[0].DocumentId);
         Assert.AreEqual("post/19", deleteResults[1].DocumentId);
         Assert.AreEqual("post/20", deleteResults[2].DocumentId);         
      }

      [TestMethod]
      public void Can_Delete_Multiple_Documents_With_Non_Existing_Ids()
      {
         var docIds = new[] {"no_such_id_1", "no_such_id_2", "no_such_id_3"};

         BatchDeleteResultCollection results = testIndex.DeleteDocuments(docIds);

         Assert.AreEqual(3, results.Count);
         Assert.AreEqual(0, results.GetFailedDocIds().Length);
         Assert.AreEqual("no_such_id_1", results[0].DocumentId);
         Assert.AreEqual("no_such_id_2", results[1].DocumentId);
         Assert.AreEqual("no_such_id_3", results[2].DocumentId);
      }

      [TestMethod]
      public void Can_Resubmit_Failed_Deletions()
      {
         var documents = new List<Document>
                            {
                               new Document("post_15").AddField("text", "I love Call of Duty"),
                               new Document("post_16").AddField("text", "I love Asteroids"),
                               new Document("post_17").AddField("text", "I love Crysis")
                            };

         testIndex.AddDocuments(documents);

         var docIds = new[] {"post_15", "post_16", "post_17"};

         BatchDeleteResultCollection results = testIndex.DeleteDocuments(docIds);

         var failedDocIds = results.GetFailedDocIds();

         if (failedDocIds.Length > 0)
         {
            testIndex.DeleteDocuments(failedDocIds);
         }         
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Add_Null_Document()
      {
         testIndex.AddDocument(null);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Delete_Multiple_Documents_With_Empty_List()
      {
         var docIds = new string[0];

         testIndex.DeleteDocuments(docIds);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Delete_Multiple_Documents_With_Null_List()
      {
         testIndex.DeleteDocuments((IEnumerable<string>) null);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Add_Multiple_Documents_With_Invalid_Document_1()
      {
         var documents = new List<Document>
                            {
                               new Document("post_18").AddField("text", "I love Call of Duty"),
                               new Document("post_19").AddField("text", "I love Asteroids"),
                               new Document("post_20").AddField("", "I love Crysis")
                            };

         testIndex.AddDocuments(documents);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Add_Multiple_Documents_With_Invalid_Document_2()
      {
         var documents = new List<Document>
                            {
                               new Document("post_18").AddField(null, "I love Call of Duty"),
                               new Document("post_19").AddField("text", "I love Asteroids"),
                               new Document("post_20").AddField("text", "I love Crysis")
                            };

         testIndex.AddDocuments(documents);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Add_Multiple_Documents_With_Invalid_Document_3()
      {
         var documents = new List<Document>
                            {
                               new Document("post_18").AddField("text", "I love Call of Duty"),
                               new Document("post_19").AddField("text", null),
                               new Document("post_20").AddField("text", "I love Crysis")
                            };

         testIndex.AddDocuments(documents);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Add_Multiple_Documents_With_Null_List()
      {
         testIndex.AddDocuments(null);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Add_Multiple_Documents_With_Empty_List()
      {
         var documents = new List<Document>();

         testIndex.AddDocuments(documents);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Delete_Document_With_Empty_DocId()
      {
         testIndex.DeleteDocument(string.Empty);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Delete_Document_With_Null_DocId()
      {
         testIndex.DeleteDocument(null);
      }

      [TestMethod]
      public void Cannot_Delete_Document_From_Non_Existing_Index()
      {
         IndexTankApiException exception = null;

         try
         {
            Index index = indexTankClient.GetIndex("no_such_index");
            index.DeleteDocument("post_1");
         }
         catch (IndexTankApiException ex)
         {
            exception = ex;
         }

         Assert.IsNotNull(exception);
         Assert.AreEqual(HttpStatusCode.NotFound, exception.GetHttpStatusCode());
      }

      [TestMethod]
      [ExpectedException(typeof(IndexTankApiException))]
      public void Cannot_Add_Document_With_Variable_Num_Too_Large()
      {
         Document document = new Document("post_4").AddField("text", "I love Angry Birds").AddVariable(3, 0.5f);

         testIndex.AddDocument(document);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void Cannot_Add_Document_With_Variable_Num_Less_Than_Zero()
      {
         Document document = new Document("post_4").AddField("text", "I love Angry Birds").AddVariable(-1, 0.5f);

         testIndex.AddDocument(document);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Add_Document_With_Null_Category_Name()
      {
         Document document = new Document("post_4").AddField("text", "I love Angry Birds").AddCategory(null,
                                                                                                       "some value");

         testIndex.AddDocument(document);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Add_Document_With_Null_Category_Value()
      {
         Document document = new Document("post_4").AddField("text", "I love Angry Birds").AddCategory("post type", null);

         testIndex.AddDocument(document);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Add_Document_With_Empty_Category_Name()
      {
         Document document = new Document("post_4").AddField("text", "I love Angry Birds").AddCategory(string.Empty,
                                                                                                       "some value");

         testIndex.AddDocument(document);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Add_Document_With_Empty_Field_Key()
      {
         Document document = new Document("post_10").AddField("", "This field has no key");

         testIndex.AddDocument(document);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Add_Document_With_Null_Field_Value()
      {
         Document document = new Document("post_11").AddField("text", null);

         testIndex.AddDocument(document);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Add_Document_With_Null_Field_Key()
      {
         Document document = new Document("post_12").AddField(null, "This field has a null key");

         testIndex.AddDocument(document);
      }

      [TestMethod]
      [ExpectedException(typeof(FormatException))]
      public void Cannot_Add_Document_If_DocId_Is_Too_Long()
      {
         Document document = new Document(GetRandomString(1025)).AddField("text", "I love Bioshock");

         testIndex.AddDocument(document);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void Cannot_Add_Document_With_Field_Value_Too_Large()
      {
         Document document = new Document("post_8").AddField("text", GetRandomString(120000));         
      }

      [TestMethod]
      [ExpectedException(typeof(InvalidOperationException))]
      public void Cannot_Add_Document_With_Combined_Field_Values_Too_Large()
      {
         Document document = new Document("post_8").AddField("text", GetRandomString(60000)).AddField("someOther",
                                                                                                      GetRandomString(
                                                                                                         60000));

         testIndex.AddDocument(document);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Add_Document_With_Empty_DocId()
      {
         Document document = new Document("").AddField("text", "I love Knock Out");

         testIndex.AddDocument(document);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Add_Document_With_Null_DocId()
      {
         Document document = new Document(null).AddField("text", "I love Grand Theft Auto");

         testIndex.AddDocument(document);
      }

      [TestMethod]
      [ExpectedException(typeof(InvalidOperationException))]
      public void Cannot_Add_Document_With_No_Fields()
      {
         var document = new Document("post_5");         

         testIndex.AddDocument(document);
      }

      [TestMethod]
      public void Cannot_Add_Document_Before_Index_Is_Initialized()
      {
         Document document = new Document("post_6").AddField("text", "I love Donkey Kong");

         Index index = indexTankClient.CreateIndex("test_index_2");

         IndexTankApiException exception = null;

         try
         {
            index.AddDocument(document);
         }
         catch (IndexTankApiException ex)
         {
            exception = ex;
         }

         Assert.IsFalse(index.IsStarted);
         Assert.IsNotNull(exception);
         Assert.AreEqual(HttpStatusCode.Conflict, exception.GetHttpStatusCode());
         Assert.AreEqual("The index is not yet initialized. Assure that the index's Started property is true before accessing it.", exception.Message);
      }

      [TestMethod]
      public void Cannot_Add_Document_To_Non_Existing_Index()
      {
         IndexTankApiException exception = null;

         Document document = new Document("post_7").AddField("text", "I love Grand Theft Auto");

         try
         {
            Index index = indexTankClient.GetIndex("no_such_index");
            index.AddDocument(document);
         }
         catch (IndexTankApiException ex)
         {
            exception = ex;
         }

         Assert.IsNotNull(exception);
         Assert.AreEqual(HttpStatusCode.NotFound, exception.GetHttpStatusCode());
      }

      private string GetRandomString(int lengthInBytes)
      {
         var random = new Random((int) DateTime.Now.Ticks);
         var builder = new StringBuilder();

         while (Encoding.UTF8.GetByteCount(builder.ToString()) < lengthInBytes)
         {
            char ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
            builder.Append(ch);
         }

         return builder.ToString();
      }
   }
}