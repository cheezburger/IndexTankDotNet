namespace IndexTankDotNet.Tests
{
   using System;
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.Linq;
   using System.Threading;
   using Microsoft.VisualStudio.TestTools.UnitTesting;

   /// <summary>
   /// These tests are designed to run against a real IndexTank Account.
   /// You must supply the private URL of an existing IndexTank account to run these tests.
   /// WARNING: RUNNING THESE TESTS will DELETE all existing indexes in the account.
   /// </summary>
   [TestClass]
   public class SearchTests
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

         testIndex.AddDocument(new Document("post_1", "I love Bioshock")
                                  .AddField("title", "I love Bioshock")
                                  .AddCategory("post type", "game review")
                                  .AddCategory("rating", "4"));

         Thread.Sleep(1000);

         testIndex.AddDocument(new Document("post_2", "I love Angry Birds")
                                  .AddField("title", "I love Angry Birds")  
                                  .AddCategory("post type", "game review")
                                  .AddCategory("rating", "5")
                                  .AddCategory("source", "user submitted"));

         Thread.Sleep(1000);

         testIndex.AddDocument(new Document("post_3", "I like Nikon")
                                  .AddCategory("post type", "camera review")
                                  .AddCategory("rating", "3"));

         Thread.Sleep(1000);

         testIndex.AddDocument(
            new Document("post_4",
                         "When in the Course of human events it becomes necessary for one people to dissolve the political bands which have connected them with another and to assume among the powers of the earth, the separate and equal station to which the Laws of Nature and of Nature's God entitle them, a decent respect to the opinions of mankind requires that they should declare the causes which impel them to the separation. We hold these truths to be self-evident, that all men are created equal, that they are endowed by their Creator with certain unalienable Rights, that among these are Life, Liberty and the pursuit of Happiness. — That to secure these rights, Governments are instituted among Men, deriving their just powers from the consent of the governed, — That whenever any Form of Government becomes destructive of these ends, it is the Right of the People to alter or to abolish it, and to institute new Government, laying its foundation on such principles and organizing its powers in such form, as to them shall seem most likely to effect their Safety and Happiness. Prudence, indeed, will dictate that Governments long established should not be changed for light and transient causes; and accordingly all experience hath shewn that mankind are more disposed to suffer, while evils are sufferable than to right themselves by abolishing the forms to which they are accustomed. But when a long train of abuses and usurpations, pursuing invariably the same Object evinces a design to reduce them under absolute Despotism, it is their right, it is their duty, to throw off such Government, and to provide new Guards for their future security. — Such has been the patient sufferance of these Colonies; and such is now the necessity which constrains them to alter their former Systems of Government. The history of the present King of Great Britain is a history of repeated injuries and usurpations, all having in direct object the establishment of an absolute Tyranny over these States. To prove this, let Facts be submitted to a candid world.")
               .AddField("title", "The unanimous Declaration of the thirteen united States of America"));

         Thread.Sleep(1000);

         testIndex.AddDocument(new Document("post_5", "Canons are okay")
                                  .AddCategory("post type", "camera review")
                                  .AddCategory("rating", "3")
                                  .AddVariable(1, 0.5f)
                                  .AddVariable(2, 1.5f));

         Thread.Sleep(1000);

         testIndex.AddDocument(new Document("post_6", "Hondas are okay")
                                  .AddCategory("post type", "motorcycle review")
                                  .AddCategory("rating", "3")
                                  .AddVariable(0, 6.3f)
                                  .AddVariable(1, 0.75f)
                                  .AddVariable(2, 10.0f));

         Thread.Sleep(1000);

         testIndex.AddDocument(new Document("post_7", "This document contains the word blueberries,").AddField("title", "All About Blueberries"));
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
      public void Can_Search_Against_Default_Field()
      {
         SearchResult result = testIndex.Search("love");

         Assert.AreEqual(2, result.Matches);
         Assert.AreEqual("love", result.QueryText);
         Assert.AreEqual(2, result.ResultDocuments.Count);
         Assert.IsTrue(result.ResultDocuments.Select(rd => rd.DocumentId).Contains("post_1"));
         Assert.IsTrue(result.ResultDocuments.Select(rd => rd.DocumentId).Contains("post_2"));
         Assert.IsTrue(result.SearchTime > 0);
         Assert.AreEqual(1, result.Facets["rating"]["5"]);
         Assert.AreEqual(1, result.Facets["rating"]["4"]);
         Assert.AreEqual(2, result.Facets["post type"]["game review"]);
      }

      [TestMethod]
      public void Can_Search_Across_Multiple_Fields()
      {
         SearchResult result = testIndex.Search(new Query("blueberries").WithSnippetFromFields("text", "title"), "text", "title");

         Assert.AreEqual(1, result.Matches);
         Assert.IsTrue(result.ResultDocuments[0].Snippets["title"].Contains("<b>Blueberries</b>"));
         Assert.IsTrue(result.ResultDocuments[0].Snippets["text"].Contains("<b>blueberries</b>"));
      }

      [TestMethod]
      public void Can_Search_Across_Multiple_Fields_With_Empty_Field()
      {
         SearchResult result = testIndex.Search(new Query("blueberries").WithSnippetFromFields("text", "title"), string.Empty, "title");

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual(string.Empty, result.ResultDocuments[0].Snippets["text"]);
         Assert.IsTrue(result.ResultDocuments[0].Snippets["title"].Contains("<b>Blueberries</b>"));

      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Search_Across_Multiple_Fields_With_Null_Field()
      {
         SearchResult result = testIndex.Search(new Query("blueberries").WithSnippetFromFields("text", "title"), "text", null);         
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Search_Across_Multiple_Fields_With_Null_Query()
      {
         SearchResult result = testIndex.Search(null, "text", "title");         
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Search_Across_Multiple_Fields_With_Null_Fields()
      {
         SearchResult result = testIndex.Search(new Query("blueberries").WithSnippetFromFields("text", "title"), null);

      }

      [TestMethod]
      public void Can_Search_Against_Other_Field()
      {
         SearchResult result = testIndex.Search(new Query("title:unanimous").WithSnippetFromFields("text", "title"));

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual("title:unanimous", result.QueryText);
         Assert.AreEqual(1, result.ResultDocuments.Count);
         Assert.IsTrue(result.ResultDocuments.Select(rd => rd.DocumentId).Contains("post_4"));
         Assert.IsTrue(result.SearchTime > 0);
         Assert.IsTrue(result.ResultDocuments[0].Snippets["title"].Contains("<b>unanimous</b>"));
         Assert.AreEqual(string.Empty, result.ResultDocuments[0].Snippets["text"]);
      }

      [TestMethod]
      public void Can_Search_With_Function()
      {
         SearchResult result = testIndex.Search(new Query("love").WithScoringFunction(5));

         Assert.AreEqual(2, result.Matches);
         Assert.AreEqual("love", result.QueryText);
         Assert.AreEqual(2, result.ResultDocuments.Count);
         Assert.IsTrue(result.ResultDocuments.Select(rd => rd.DocumentId).Contains("post_1"));
         Assert.IsTrue(result.ResultDocuments.Select(rd => rd.DocumentId).Contains("post_2"));
         Assert.IsTrue(result.SearchTime > 0);
         Assert.AreEqual(1, result.Facets["rating"]["5"]);
         Assert.AreEqual(1, result.Facets["rating"]["4"]);
         Assert.AreEqual(2, result.Facets["post type"]["game review"]);
      }

      [TestMethod]
      public void Can_Perform_Case_Insensitive_Search_By_Default()
      {
         SearchResult result = testIndex.Search(new Query("LOVE").WithScoringFunction(1));

         Assert.AreEqual(2, result.Matches);
         Assert.AreEqual("LOVE", result.QueryText);
         Assert.AreEqual(2, result.ResultDocuments.Count);
         Assert.IsTrue(result.ResultDocuments.Select(rd => rd.DocumentId).Contains("post_1"));
         Assert.IsTrue(result.ResultDocuments.Select(rd => rd.DocumentId).Contains("post_2"));
         Assert.IsTrue(result.SearchTime > 0);
         Assert.AreEqual(1, result.Facets["rating"]["5"]);
         Assert.AreEqual(1, result.Facets["rating"]["4"]);
         Assert.AreEqual(2, result.Facets["post type"]["game review"]);
      }

      [TestMethod]
      public void Can_Search_With_Snippet()
      {
         SearchResult result = testIndex.Search(new Query("object").WithSnippetFromFields("text"));

         Assert.AreEqual(1, result.Matches);
         Assert.IsTrue(result.ResultDocuments[0].Snippets["text"].Contains("<b>Object</b>"));
      }

      [TestMethod]
      public void Cannot_Search_With_Empty_Snippet()
      {
         SearchResult result = testIndex.Search(new Query("object").WithSnippetFromFields("text", string.Empty));
         Assert.AreEqual(1, result.Matches);
         Assert.IsTrue(result.ResultDocuments[0].Snippets["text"].Contains("<b>Object</b>"));
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Search_With_Null_Snippet()
      {
         SearchResult result = testIndex.Search(new Query("object").WithSnippetFromFields("text", null));         
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Search_With_Null_Fetch_Field()
      {
         SearchResult result = testIndex.Search(new Query("object").WithFields(null));
      }

      [TestMethod]
      public void Can_Search_With_Empty_Fetch_Field()
      {
         SearchResult result = testIndex.Search(new Query("object").WithFields(string.Empty));
         Assert.AreEqual(1, result.Matches);
         Assert.IsNull(result.ResultDocuments[0].Fields);
      }

      [TestMethod]
      public void Can_Search_With_Discrete_Fetch_Fields()
      {
         SearchResult result = testIndex.Search(new Query("bioshock").WithFields("text"));

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual("I love Bioshock", result.ResultDocuments[0].Fields["text"]);
      }

      [TestMethod]
      public void Can_Search_With_Timestamp_Fetch_Field()
      {
         SearchResult result = testIndex.Search(new Query("bioshock").WithFields("timestamp"));

         Assert.AreEqual(1, result.Matches);
         Assert.IsTrue(result.ResultDocuments[0].Fields.ContainsKey("timestamp"));
         Assert.IsTrue(Convert.ToInt32(result.ResultDocuments[0].Fields["timestamp"]) > 0);
      }

      [TestMethod]
      public void Can_Search_With_All_Fetch_Fields()
      {
         SearchResult result = testIndex.Search(new Query("love").WithFields("*"));

         Assert.AreEqual(2, result.Matches);
         Assert.IsTrue(result.ResultDocuments[0].Fields.ContainsKey("text"));
         Assert.IsTrue(result.ResultDocuments[1].Fields.ContainsKey("text"));
         Assert.IsTrue(result.ResultDocuments[0].Fields.ContainsKey("timestamp"));
         Assert.IsTrue(result.ResultDocuments[1].Fields.ContainsKey("timestamp"));
      }

      [TestMethod]
      public void Can_Search_With_Multiple_Fetch_Fields_And_Multiple_Snippets()
      {
         SearchResult result = testIndex.Search(new Query("love").WithFields("*").WithSnippetFromFields("text"));

         Assert.AreEqual(2, result.Matches);
         Assert.IsTrue(result.ResultDocuments[0].Fields.ContainsKey("text"));
         Assert.IsTrue(result.ResultDocuments[0].Fields.ContainsKey("timestamp"));
         Assert.IsTrue(result.ResultDocuments[0].Snippets.ContainsKey("text"));
         Assert.IsTrue(result.ResultDocuments[1].Fields.ContainsKey("text"));
         Assert.IsTrue(result.ResultDocuments[1].Fields.ContainsKey("timestamp"));
         Assert.IsTrue(result.ResultDocuments[1].Snippets.ContainsKey("text"));
         Assert.AreEqual("I <b>love</b> Angry Birds", result.ResultDocuments[0].Snippets["text"]);
         Assert.AreEqual("I <b>love</b> Bioshock", result.ResultDocuments[1].Snippets["text"]);
      }

      [TestMethod]
      public void Cannot_Obtain_Fields_That_Do_Not_Exist_In_Document()
      {
         SearchResult result = testIndex.Search(new Query("bioshock").WithFields("text", "author"));

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual("I love Bioshock", result.ResultDocuments[0].Fields["text"]);
         Assert.IsFalse(result.ResultDocuments[0].Fields.ContainsKey("author"));
      }

      [TestMethod]
      public void Cannot_Obtain_Snippets_That_Do_Not_Exist_In_Document()
      {
         SearchResult result = testIndex.Search(new Query("object").WithSnippetFromFields("text", "other"));

         Assert.AreEqual(1, result.Matches);
         Assert.IsTrue(result.ResultDocuments[0].Snippets["text"].Contains("<b>Object</b>"));
         Assert.IsFalse(result.ResultDocuments[0].Snippets.ContainsKey("other"));
      }

      [TestMethod]
      public void Cannot_Obtain_Fields_That_Are_Not_Explicitly_Requested()
      {
         SearchResult result = testIndex.Search(new Query("bioshock"));

         Assert.AreEqual(1, result.Matches);
         Assert.IsNull(result.ResultDocuments[0].Fields);
      }

      [TestMethod]
      public void Cannot_Obtain_Snippets_That_Are_Not_Explicitly_Requested()
      {
         SearchResult result = testIndex.Search(new Query("object"));

         Assert.AreEqual(1, result.Matches);
         Assert.IsNull(result.ResultDocuments[0].Snippets);
      }

      [TestMethod]
      public void Can_Search_With_Multiple_Snippets()
      {
         SearchResult result = testIndex.Search(new Query("object").WithSnippetFromFields("text", "title"));
         Assert.AreEqual(1, result.Matches);
         Assert.IsTrue(result.ResultDocuments[0].Snippets["text"].Contains("<b>Object</b>"));
         Assert.AreEqual(string.Empty, result.ResultDocuments[0].Snippets["title"]);
      }

      [TestMethod]
      public void Can_Search_Against_Default_Field_With_UrlEncoded_Query()
      {
         SearchResult result = testIndex.Search(new Query("like Nikon"));

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual("like Nikon", result.QueryText);
         Assert.AreEqual(1, result.ResultDocuments.Count);
         Assert.IsTrue(result.ResultDocuments.Select(rd => rd.DocumentId).Contains("post_3"));
         Assert.IsTrue(result.SearchTime > 0);
         Assert.AreEqual(1, result.Facets["rating"]["3"]);
         Assert.AreEqual(1, result.Facets["post type"]["camera review"]);
      }

      [TestMethod]
      public void Can_Page_Results()
      {
         SearchResult result = testIndex.Search(new Query("I").Skip(1));

         Assert.AreEqual(3, result.Matches);
         Assert.AreEqual("I", result.QueryText);
         Assert.AreEqual(2, result.ResultDocuments.Count);
         Assert.IsTrue(result.ResultDocuments.Select(rd => rd.DocumentId).Contains("post_1"));
         Assert.IsTrue(result.ResultDocuments.Select(rd => rd.DocumentId).Contains("post_2"));
         Assert.IsFalse(result.ResultDocuments.Select(rd => rd.DocumentId).Contains("post_3"));
         Assert.IsTrue(result.SearchTime > 0);
         Assert.AreEqual(1, result.Facets["rating"]["5"]);
         Assert.AreEqual(2, result.Facets["post type"]["game review"]);
         Assert.AreEqual(1, result.Facets["post type"]["camera review"]);
      }

      [TestMethod]
      public void Can_Limit_Number_Of_Results_To_Return()
      {
         SearchResult result = testIndex.Search(new Query("love").Take(1));

         Assert.AreEqual(result.Matches, 2);
         Assert.AreEqual(result.QueryText, "love");
         Assert.AreEqual(1, result.ResultDocuments.Count);
         Assert.IsTrue(result.ResultDocuments.Select(rd => rd.DocumentId).Contains("post_2"));
         Assert.IsTrue(result.SearchTime > 0);
         Assert.AreEqual(1, result.Facets["rating"]["5"]);
         Assert.AreEqual(1, result.Facets["rating"]["4"]);
         Assert.AreEqual(2, result.Facets["post type"]["game review"]);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void Cannot_Search_If_Sum_Of_Skip_And_Take_Exceeds_5k()
      {
         SearchResult result = testIndex.Search(new Query("love").Skip(2501).Take(2500));
      }

      [TestMethod]
      public void Can_Search_With_Unset_Fetch_Variables()
      {
         SearchResult result = testIndex.Search(new Query("love").WithVariables());

         Assert.AreEqual(2, result.Matches);
         Assert.AreEqual(0f, result.ResultDocuments[0].Variables[0]);
         Assert.AreEqual(0f, result.ResultDocuments[0].Variables[1]);
         Assert.AreEqual(0f, result.ResultDocuments[0].Variables[2]);
         Assert.AreEqual(0f, result.ResultDocuments[1].Variables[0]);
         Assert.AreEqual(0f, result.ResultDocuments[1].Variables[1]);
         Assert.AreEqual(0f, result.ResultDocuments[1].Variables[2]);
      }

      [TestMethod]
      public void Can_Search_With_Set_Fetch_Variables()
      {
         SearchResult result = testIndex.Search(new Query("okay").WithVariables());

         Assert.AreEqual(2, result.Matches);
         Assert.AreEqual(6.3f, result.ResultDocuments[0].Variables[0]);
         Assert.AreEqual(0.75f, result.ResultDocuments[0].Variables[1]);
         Assert.AreEqual(10.0f, result.ResultDocuments[0].Variables[2]);
         Assert.AreEqual(0.0f, result.ResultDocuments[1].Variables[0]);
         Assert.AreEqual(0.5f, result.ResultDocuments[1].Variables[1]);
         Assert.AreEqual(1.5f, result.ResultDocuments[1].Variables[2]);
      }

      [TestMethod]
      public void Can_Search_With_Fetch_Categories()
      {
         SearchResult result = testIndex.Search(new Query("love").WithCategories());

         Assert.AreEqual("user submitted", result.ResultDocuments[0].Categories["source"]);
         Assert.AreEqual("5", result.ResultDocuments[0].Categories["rating"]);
         Assert.AreEqual("game review", result.ResultDocuments[0].Categories["post type"]);
         Assert.AreEqual("4", result.ResultDocuments[1].Categories["rating"]);
         Assert.AreEqual("game review", result.ResultDocuments[1].Categories["post type"]);
      }

      [TestMethod]
      public void Can_Search_With_Query_Variables()
      {
         SearchResult result = testIndex.Search(new Query("love").WithQueryVariable(0, 5.2f).WithQueryVariable(2, 1.1f));

         Assert.AreEqual(2, result.Matches);
      }

      [TestMethod]
      public void Can_Search_With_NonZero_Query_Variable()
      {
         SearchResult result = testIndex.Search(new Query("love").WithQueryVariable(-1, 5.2f));

         Assert.AreEqual(2, result.Matches);
      }

      [TestMethod]
      public void Can_Search_With_Category_Filter()
      {
         SearchResult result = testIndex.Search(new Query("love").WithCategoryFilter("rating", "3", "4"));

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual("post_1", result.ResultDocuments[0].DocumentId);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Search_With_Category_Filter_Null_Name()
      {
         SearchResult result = testIndex.Search(new Query("love").WithCategoryFilter(null, "3", "4"));         
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Search_With_Category_Filter_Empty_Name()
      {
         SearchResult result = testIndex.Search(new Query("love").WithCategoryFilter(string.Empty, "3", "4"));
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Search_With_Category_Filter_Null_Match()
      {
         SearchResult result = testIndex.Search(new Query("love").WithCategoryFilter("rating", null));
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Search_With_Category_Filter_Null_Match_Name()
      {
         SearchResult result = testIndex.Search(new Query("love").WithCategoryFilter("rating", "3", null));
      }

      [TestMethod]
      public void Can_Search_With_Category_Filter_Empty_Match_Name()
      {
         SearchResult result = testIndex.Search(new Query("love").WithCategoryFilter("rating", "5", string.Empty));

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual("post_2", result.ResultDocuments[0].DocumentId);
      }      

      [TestMethod]
      public void Can_Search_With_Multiple_Category_Filters()
      {
         SearchResult result = testIndex.Search(new Query("love").WithCategoryFilter("rating", "4", "5").WithCategoryFilter("source", "user submitted"));

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual("post_2", result.ResultDocuments[0].DocumentId);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Search_With_Null_Query_Text()
      {
         testIndex.Search(new Query(null));
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Search_With_Empty_Query()
      {
         testIndex.Search(new Query(string.Empty));
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Search_With_Whitespace_Query()
      {
         testIndex.Search(new Query("   "));
      }

      [TestMethod]
      public void Can_Search_With_Leading_Space_Query()
      {
         SearchResult result = testIndex.Search(new Query("   course").WithSnippetFromFields("text").WithFields("title"));

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual("The unanimous Declaration of the thirteen united States of America", result.ResultDocuments[0].Fields["title"]);
         Assert.IsTrue(result.ResultDocuments[0].Snippets["text"].Contains("<b>Course</b>"));
      }

      [TestMethod]
      public void Can_Search_With_Trailing_Space_Query()
      {
         SearchResult result = testIndex.Search(new Query("course    ").WithSnippetFromFields("text").WithFields("title"));

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual("The unanimous Declaration of the thirteen united States of America", result.ResultDocuments[0].Fields["title"]);
         Assert.IsTrue(result.ResultDocuments[0].Snippets["text"].Contains("<b>Course</b>"));
      }

      [TestMethod]
      public void Can_Search_With_Variable_Range()
      {
         SearchResult result = testIndex.Search(new Query("okay").WithDocumentVariableFilter(1, 0.6f, 1.0f));

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual("post_6", result.ResultDocuments.Single().DocumentId);
      }

      [TestMethod]
      public void Can_Search_With_Variable_Range_With_No_Lowerbound()
      {
         SearchResult result = testIndex.Search(new Query("okay").WithDocumentVariableFilter(1, float.NegativeInfinity, 0.6f));

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual("post_5", result.ResultDocuments.Single().DocumentId);
      }

      [TestMethod]
      public void Can_Search_With_Variable_Range_With_No_Upperbound()
      {
         SearchResult result = testIndex.Search(new Query("okay").WithDocumentVariableFilter(1, 0.6f, float.PositiveInfinity));

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual("post_6", result.ResultDocuments.Single().DocumentId);
      }

      [TestMethod]
      public void Can_Search_With_Multiple_Variable_Ranges()
      {
         SearchResult result = testIndex.Search(new Query("okay").WithDocumentVariableFilter(1, 0.4f, 0.6f).WithDocumentVariableFilter(1, 2f, 4f));

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual("post_5", result.ResultDocuments.Single().DocumentId);
      }

      [TestMethod]
      public void Can_Search_With_Function_Range()
      {
         testIndex.AddFunction(1, "d[1] * 100");

         SearchResult result = testIndex.Search(new Query("okay").WithFunctionFilter(1, 60, 100));

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual("post_6", result.ResultDocuments.Single().DocumentId);
      }

      [TestMethod]
      public void Can_Search_With_Function_Range_With_No_Lowerbound()
      {
         testIndex.AddFunction(1, "d[1] * 100");

         SearchResult result = testIndex.Search(new Query("okay").WithFunctionFilter(1, double.NegativeInfinity, 60));

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual("post_5", result.ResultDocuments.Single().DocumentId);
      }

      [TestMethod]
      public void Can_Search_With_Function_Range_With_No_Upperbound()
      {
         testIndex.AddFunction(1, "d[1] * 100");

         SearchResult result = testIndex.Search(new Query("okay").WithFunctionFilter(1, 60, double.PositiveInfinity));

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual("post_6", result.ResultDocuments.Single().DocumentId);
      }

      [TestMethod]
      public void Can_Search_With_Multiple_Function_Ranges()
      {
         testIndex.AddFunction(1, "d[1] * 100");

         SearchResult result = testIndex.Search(new Query("okay").WithFunctionFilter(1, 10, 60).WithFunctionFilter(1, 80, 100));

         Assert.AreEqual(1, result.Matches);
         Assert.AreEqual("post_5", result.ResultDocuments.Single().DocumentId);
      }

      [TestMethod]
      public void Can_Delete_By_Query()
      {
         var query = new Query("okay");

         SearchResult resultBefore = testIndex.Search(query);

         bool deleteResult = testIndex.DeleteDocuments(query);

         SearchResult resultAfter = testIndex.Search(query);

         Assert.IsTrue(deleteResult);
         Assert.AreEqual(2, resultBefore.Matches);
         Assert.AreEqual(0, resultAfter.Matches);
      }

      [TestMethod]
      [ExpectedException(typeof(NotSupportedException))]
      public void Cannot_Delete_By_Query_With_Take()
      {
         var query = new Query("okay").Take(3);

         bool deleteResult = testIndex.DeleteDocuments(query);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Delete_By_Query_With_Null_Query()
      {
         testIndex.DeleteDocuments((Query) null);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Search_With_Null_Query()
      {
         testIndex.Search((Query) null);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Search_With_Null_String()
      {
         testIndex.Search((string)null);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Search_With_Empty_String()
      {
         testIndex.Search(string.Empty);
      }

      [TestMethod]
      public void Can_Promote_Result()
      {
         var query = new Query("love");

         SearchResult resultBefore = testIndex.Search(query);

         bool promoteResult = testIndex.PromoteDocument("post_1", "love");

         SearchResult resultAfter = testIndex.Search(query);

         Assert.IsTrue(promoteResult);
         Assert.AreEqual(2, resultBefore.Matches);
         Assert.AreEqual(2, resultAfter.Matches);
         Assert.AreEqual("post_2", resultBefore.ResultDocuments[0].DocumentId);
         Assert.AreEqual("post_1", resultAfter.ResultDocuments[0].DocumentId);
      }

      [TestMethod]
      public void Promote_Result_For_Search_In_Other_Field()
      {
         var query = new Query("love");

         SearchResult resultBefore = testIndex.Search(query, "title");

         bool promoteResult = testIndex.PromoteDocument("post_1", "title:love");

         SearchResult resultAfter = testIndex.Search(query, "title");

         Assert.IsTrue(promoteResult);
         Assert.AreEqual(2, resultBefore.Matches);
         Assert.AreEqual(2, resultAfter.Matches);
         Assert.AreEqual("post_2", resultBefore.ResultDocuments[0].DocumentId);
         Assert.AreEqual("post_1", resultAfter.ResultDocuments[0].DocumentId);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Promote_Result_With_Null_DocId()
      {
         testIndex.PromoteDocument(null, "love");         
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Promote_Result_With_Empty_DocId()
      {
         testIndex.PromoteDocument(string.Empty, "love");
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Cannot_Promote_Result_With_Null_QueryText()
      {
         testIndex.PromoteDocument("post_1", null);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void Cannot_Promote_Result_With_Empty_QueryText()
      {
         testIndex.PromoteDocument("post_1", string.Empty);
      }

      [TestMethod]
      [Ignore]
      public void Can_Set_Timeout_On_Search()
      {
         var query = new Query("love");
         
         SearchResult result = testIndex.Search(query, 1000);

         Assert.AreEqual(2, result.Matches);
         Assert.AreEqual("love", result.QueryText);
      }      

      [TestMethod]
      [Ignore]
      public void Cannot_Exceed_Timeout_On_Search()
      {
         Exception exception = null;

         try
         {
            var query = new Query("love");
            SearchResult result = testIndex.Search(query, 25);

         }
         catch (TimeoutException ex)
         {
            // Notify user that search timed out
            exception = ex;
         }
         
         Assert.IsNotNull(exception);
         Assert.IsInstanceOfType(exception, typeof(TimeoutException));                  
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void Cannot_Specify_Zero_Timeout()
      {
         var query = new Query("love");

         SearchResult result = testIndex.Search(query, 0);

      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void Cannot_Specify_Negative_Timeout()
      {
         var query = new Query("love");

         SearchResult result = testIndex.Search(query, -1);

      }
   }
}