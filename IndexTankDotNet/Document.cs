namespace IndexTankDotNet
{
   using System;
   using System.Collections.Generic;
   using System.Globalization;
   using System.Text;
   using Newtonsoft.Json;

   /// <summary>
   /// An object that holds content that can be stored in an IndexTank index. 
   /// </summary>
   [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
   public class Document
   {
      [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
      private IDictionary<string, string> categories;

      [JsonProperty(NullValueHandling = NullValueHandling.Ignore, Required = Required.Always)]
      private Dictionary<string, string> fields;

      [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
      private IDictionary<int, float> variables;

      /// <summary>
      /// Initializes a new instance of the Document class.
      /// </summary>
      /// <param name="documentId">The identifier for the document. If you specify an identifier that already exists in the document, the existing document will be overwritten with values from the new document.</param>
      /// <exception cref="ArgumentNullException">Thrown when the documentId is null.</exception>
      /// <exception cref="ArgumentException">Thrown when the documentId is an empty string, or consists of only whitespace.</exception>
      /// <exception cref="FormatException">Thrown when the documentId exceeds 1024 bytes.</exception>
      public Document(string documentId)
      {
         if (documentId == null)
         {
            throw new ArgumentNullException("documentId", "The document identifier is null.");
         }

         if (string.IsNullOrWhiteSpace(documentId))
         {
            throw new ArgumentException("The document identifier is empty.", "documentId");
         }

         if (Encoding.ASCII.GetByteCount(documentId) > 1024)
         {
            throw new FormatException("The document identifier exceeds 1024 bytes.");
         }

         DocumentId = documentId;
      }

      /// <summary>
      /// Initializes a new instance of the Document class, and populates it with content for the document's "text" field.
      /// <para>Same as calling: new Document(documentId).AddField("text", text)</para>
      /// </summary>
      /// <param name="documentId">The unique identifier for the document.</param>
      /// <param name="text">The textual content for the document's "text" field. The "text" field is the default field used for search queries.</param>
      /// <seealso cref="Document(string)"/>
      /// <seealso cref="AddField(string, string)"/>
      /// <exception cref="ArgumentNullException">Thrown when the documentId is null.</exception>
      /// <exception cref="ArgumentException">Thrown when the documentId is an empty string, or consists of only whitespace.</exception>
      /// <exception cref="FormatException">Thrown when the documentId exceeds 1024 bytes.</exception>
      public Document(string documentId, string text) : this(documentId)
      {
         AddField("text", text);
      }

      /// <summary>
      /// Gets the identifier for the document.
      /// </summary>
      [JsonProperty(PropertyName = "docid")]
      public string DocumentId { get; private set; }

      internal IDictionary<string, string> Fields
      {
         get { return fields; }
      }

      /// <summary>
      /// Attaches a variable to a document.
      /// </summary>
      /// <param name="variableNumber">The position of the variable on the document. If a variable with the same position already exists on the document, its value will be overwritten with the value you specify in this method. To simply update variables on an existing document, get a reference to the index the contains the document and use the UpdateVariable method instead.</param>
      /// <param name="value">The value that will be used in calculations that use this variable.</param>
      /// <returns>Returns the Document.</returns>
      /// <exception cref="ArgumentOutOfRangeException">Thrown when the variable number is less than zero.</exception>
      public Document AddVariable(int variableNumber, float value)
      {
         if (variableNumber < 0)
         {
            throw new ArgumentOutOfRangeException("variableNumber", variableNumber, "The variable number is less than zero.");
         }

         if (variables == null)
         {
            variables = new Dictionary<int, float>();
         }

         variables[variableNumber] = value;

         return this;
      }

      /// <summary>
      /// Attaches a field to a document.
      /// </summary>
      /// <param name="fieldName">The name of the field. If a field with the same name already exists on the document, its value will be overwritten with the value you specify in this method.</param>
      /// <param name="text">The textual content of the field.</param>
      /// <returns>Returns the Document.</returns>
      /// <exception cref="ArgumentNullException">Thrown when fieldName or text is null.</exception>
      /// <exception cref="ArgumentException">Thrown when fieldName is an empty string or consists of only whitespace; or when fieldName is "timestamp" and text is an empty string or consists of only whitespace.</exception>
      /// <exception cref="ArgumentOutOfRangeException">Thrown when text exceeds 100 kbytes; or when fieldName is "timestamp" and text exceeds the value that can be converted to an Int32.</exception>
      /// <exception cref="FormatException">Thrown when fieldName is "timestamp" and text contains a value that cannot be converted to an Int32.</exception>
      public Document AddField(string fieldName, string text)
      {
         if (fieldName == null)
         {
            throw new ArgumentNullException("fieldName", "The field name is null");
         }

         if (string.IsNullOrWhiteSpace(fieldName))
         {
            throw new ArgumentException("The field name is empty.", "fieldName");
         }

         if (text == null)
         {
            throw new ArgumentNullException("text", "The field value is null.");
         }

         if (fieldName == "timestamp")
         {
            if (string.IsNullOrWhiteSpace(text))
            {
               throw new ArgumentException("The fieldName is \"timestamp\", but text is an empty string.");
            }

            try
            {
               text = Convert.ToInt32(text, CultureInfo.InvariantCulture).ToString();
            }
            catch (OverflowException)
            {
               throw new ArgumentOutOfRangeException("text", "The fieldName is \"timestamp\", but text exceeds the value that can be converted to an Int32.");
            }
            catch (FormatException)
            {
               throw new FormatException("The fieldName is \"timestamp\", but text contains a value that cannot be converted to an Int32.");
            }                        
         }
         
         int byteCount = Encoding.UTF8.GetByteCount(text);

         if (byteCount > 100000)
         {
            throw new ArgumentOutOfRangeException(
               "text",
               string.Format(CultureInfo.CurrentCulture, "The size of the supplied field text is {0} bytes. The field text cannot exceed 100 kbytes.", byteCount));
         }

         if (fields == null)
         {
            fields = new Dictionary<string, string>();
         }

         fields[fieldName.ToUTF8()] = text.ToUTF8();

         return this;
      }

      /// <summary>
      /// Attaches a category to a document. get a reference to the index the contains the document and use the UpdateVariables method instead.
      /// </summary>
      /// <param name="categoryName">The name of the category. If a category with the same name already exists on the document, its value will be overwritten with the value you specify in this method. To simply update categories on an existing document, get a reference to the index the contains the document and use the UpdateCategory method instead.</param>
      /// <param name="value">Text that represents the category's value..</param>
      /// <returns>Returns the Document.</returns>
      /// <exception cref="ArgumentNullException">Thrown when categoryName or value is null.</exception>
      /// <exception cref="ArgumentException">Thrown when categoryName is empty, or consists of only whitespace.</exception>
      public Document AddCategory(string categoryName, string value)
      {
         if (categoryName == null)
         {
            throw new ArgumentNullException("categoryName", "The category name is null.");
         }

         if (string.IsNullOrWhiteSpace(categoryName))
         {
            throw new ArgumentException("The category name is empty.", "categoryName");
         }

         if (value == null)
         {
            throw new ArgumentNullException("value", "The category value is null.");
         }

         if (categories == null)
         {
            categories = new Dictionary<string, string>();
         }

         categories[categoryName.ToUTF8()] = value.ToUTF8();

         return this;
      }

      /// <summary>
      /// Attach a date and time that is expected to contain the publication date of the document. If this method is not called, the time the document was added to the index will be used. This data is used to calculate the default sorting for search query results, which is that newer documents are listed first.
      /// <para>Same as calling AddField("timestamp", "&lt;seconds between Unix epoch and desired time&gt;")</para>
      /// </summary>
      /// <param name="dateTime">The date and time of the publication of the document.</param>
      /// <returns>Returns the Document.</returns>
      /// <seealso cref="AddField(string,string)"/>
      /// <exception cref="ArgumentOutOfRangeException">Thrown when a DateTime is supplied such that the number of seconds since or until the Unix epoch (January 1, 1970 00:00 UTC) is greater than that which can be expressed by a 32-bit integer.</exception>
      public Document AddTimestamp(DateTime dateTime)
      {
         double secondsSinceEpoch = Math.Round(
            (dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds, 0, MidpointRounding.ToEven);

         if (secondsSinceEpoch > Convert.ToDouble(int.MaxValue) || secondsSinceEpoch < Convert.ToDouble(int.MinValue))
         {
            throw new ArgumentOutOfRangeException("dateTime", dateTime, "The timestamp exceeds the allowed value.");
         }

         AddField("timestamp", Convert.ToInt32(secondsSinceEpoch).ToString(CultureInfo.InvariantCulture));

         return this;
      }
   }
}