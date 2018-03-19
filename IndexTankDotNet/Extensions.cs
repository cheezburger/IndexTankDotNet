namespace IndexTankDotNet
{
   using System;
   using System.ComponentModel;
   using System.Text;
   using System.Threading;

   internal static class Extensions
   {
      internal static string ToUTF8(this string unicodeString)
      {
         byte[] unicodeBytes = Encoding.Unicode.GetBytes(unicodeString);

         byte[] utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, unicodeBytes);

         return Encoding.UTF8.GetString(utf8Bytes);
      }

      internal static string ToRequestString(this double bound)
      {
         return double.IsInfinity(bound) ? "*" : bound.ToString();
      }

      internal static string ToRequestString(this float bound)
      {
         return float.IsInfinity(bound) ? "*" : bound.ToString();
      }      
   }
}