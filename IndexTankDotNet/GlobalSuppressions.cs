// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

using System.Diagnostics.CodeAnalysis;

[assembly:
   SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
      Target = "IndexTankDotNet.BatchDeleteResultCollection.#GetFailedDocIds()")]
[assembly:
   SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
      Target = "IndexTankDotNet.BatchIndexResultCollection.#GetFailedDocuments()")]
[assembly: SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member", Target = "IndexTankDotNet.Index.#GetFunctions()")]
[assembly:
   SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Scope = "member",
      Target = "IndexTankDotNet.Index.#Search(IndexTankDotNet.Query)")]
[assembly:
   SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Scope = "type", Target = "IndexTankDotNet.IndexTankApiException")]
[assembly:
   SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IndexTank", Scope = "member",
      Target = "IndexTankDotNet.IndexTankClient.#CreateIndex(System.String,System.Boolean,System.Boolean)")]
[assembly:
   SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member", Target = "IndexTankDotNet.IndexTankClient.#GetIndexes()")]
[assembly:
   SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IndexTank", Scope = "member",
      Target = "IndexTankDotNet.IndexTankClient.#HandleResponse(RestSharp.IRestResponse)")]
[assembly:
   SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
      Target = "IndexTankDotNet.IndexTankException.#GetHttpStatusCode()")]
[assembly:
   SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Scope = "type", Target = "IndexTankDotNet.IndexTankProtocolException")]
[assembly:
   SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Scope = "member",
      Target = "IndexTankDotNet.IndexTankClient.#CreateIndex(System.String,System.Boolean,System.Boolean)")]
[assembly:
   SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "IndexTankDotNet.SearchResult.#Facets")]