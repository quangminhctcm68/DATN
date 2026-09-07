using System;
using System.Globalization;

namespace Singular
{
    public static class Utilities
    {
        public static string[][] DelimitedStringsArrayToArrayOfArrayOfString(string[] delimitedStringsArray, char delimiter)
        {
            if (delimitedStringsArray == null || delimitedStringsArray.Length == 0)
            {
                SingularUnityLogger.LogDebug("push notification paths strings array is null or empty. skipping.");
                return null;
            }
            
            string[][] arrayOfArrayOfString = new string[delimitedStringsArray.Length][];
            for (int i = 0; i < delimitedStringsArray.Length; i++)
            {
                arrayOfArrayOfString[i] = delimitedStringsArray[i].Split(delimiter);
            }
            
            return arrayOfArrayOfString;
        }
        
        public static string ToCultureInvariantString(object value)
        {
            if (value is IFormattable formattable)
            {
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            }
            
            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }

    }
}