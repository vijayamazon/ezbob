using System;
using System.Globalization;

namespace ExperianLib.CaisFile
{
    public class Utils
    {
        public static string GetPaddingString(string input, int size, bool isLeft = false, bool padWithZeros = false)
        {
			if(string.IsNullOrEmpty(input) && padWithZeros) return new string('0', size);
            if (input == null) return String.Empty.PadRight(size);
            if (input.Length < size)
            {
                if(isLeft) return input.PadRight(size);
                return input.PadLeft(size);
            }
            return input.Length > size ? input.Substring(0, size) : input;
        }

        //-----------------------------------------------------------------------------------
        public static string GetPaddingString(int input, int size, bool isLeft = false)
        {
            return input.ToString(new string('0', size)).Substring(0, size);
        }

        //-----------------------------------------------------------------------------------
        public static string GetPaddingString(decimal input, int size, bool isLeft = false)
        {
            return input.ToString(new string('0', size)).Substring(0,size);
        }

        //-----------------------------------------------------------------------------------
        public static string GetPaddingString(DateTime? input, int size=8, bool isLeft = false)
        {
            var d = DateTime.MinValue;
            if (input != null) d = input.Value;
            return d == DateTime.MinValue ? "00000000" : d.ToString("ddMMyyyy");
        }

        //-----------------------------------------------------------------------------------
        public static string ToString(string input, int start, int size)
        {
            return input.Substring(start, size).Trim();
        }

        //-----------------------------------------------------------------------------------
        public static string ReabStructure(string input, int start, int size)
        {
            return input.Substring(start, size);
        }

        //-----------------------------------------------------------------------------------
        public static DateTime ToDate(string input, int start, int length = 8)
        {
            var dateStr = input.Substring(start, 8);
            if (dateStr == "00000000") return DateTime.MinValue;
            return DateTime.ParseExact(input.Substring(start, 8), "ddMMyyyy", CultureInfo.InvariantCulture);
        }

        //-----------------------------------------------------------------------------------
        public static int ToInt32(string input, int start, int size)
        {
            int result;
            int.TryParse(input.Substring(start, size),out result);
            return result;
        }

        //-----------------------------------------------------------------------------------
        public static decimal ToDecimal(string input, int start, int size)
        {
            decimal result;
            Decimal.TryParse(input.Substring(start, size), out result);
            return result;
        }
    }
}
