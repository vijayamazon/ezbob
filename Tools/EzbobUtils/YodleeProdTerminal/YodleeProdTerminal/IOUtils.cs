using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace com.yodlee.sampleapps.util
{
    class IOUtils
    {
        /**
        * Read String
        *
        * @param defaultStr
        * @return
        */
        public static string readStr(String defaultStr)
        {
            String str = readStr();
            if (str == null)
            {
                return defaultStr;
            }
            else
            {
                return str;
            }
        }

        /**
         * Read String
         *
         * @return string entered by user
         */
        public static string readStr()
        {

            String readStr = null;

            readStr = System.Console.ReadLine();
            readStr = readStr.Trim();

            return readStr;

        }

        /**
         * Read Integer
         *
         * @return
         */
        public static int readInt()
        {
            String readStr = null;

            readStr = System.Console.ReadLine();
            readStr = readStr.Trim();

            int ret = 0;
            bool isSucess = int.TryParse(readStr, out ret);

            if (!isSucess)
            {
                System.Console.WriteLine("Invalid entry: " + readStr + ". You must enter a number.");
                return -1;
            }

            return ret;
        }

        /**
         * Read Long
         *
         * @return
         */
        public static long readLong()
        {
            String readStr = null;

            readStr = System.Console.ReadLine();
            readStr = readStr.Trim();

            long ret = 0;
            bool isSucess = long.TryParse(readStr, out ret);

            if (ret.Equals(0))
            {
                System.Console.WriteLine("Invalid entry: " + readStr + ". You must enter a number.");
                return -1;
            }

            return ret;
        }

        public static decimal readDecimal()
        {
            String readStr = null;

            readStr = System.Console.ReadLine();
            readStr = readStr.Trim();

            decimal ret = 0;
            bool isSucess = decimal.TryParse(readStr, out ret);

            if (ret.Equals(0))
            {
                System.Console.WriteLine("Invalid entry: " + readStr + ". You must enter a number.");
                return -1;
            }

            return ret;
        }

        public static void clrScrn()
        {
            /*
            * for(int i=0; i<80; i++){ System.out.println(); }
            */
            System.Console.WriteLine("\n\n\n");
        }

        /**
         * Get current timestamp
         *
         * @return timestamp
         */
        public static String getTimeStamp()
        {
            DateTime dtNow = DateTime.Now;
            return dtNow.ToString("yyyyMMdd");
        }

        /**
         * Prompts input from the end user.
         *
         * @param displayInputStr
         *            Message to be displayed to the user
         * @param reenterDisplayInputStr
         *            Error Message to be display to re-enter
         * @return Input entered by the User
         */
        public static String promptInput(string displayInputStr, string reenterDisplayInputStr)
        {

            string input = null;
            System.Console.Write(displayInputStr);
            bool validInputFlag = false;

            do
            {
                validInputFlag = true;

                input = (IOUtils.readStr()).Trim();
                if (null != input)
                {

                    validInputFlag = false;
                }

                if (validInputFlag && (null != reenterDisplayInputStr))
                {
                    System.Console.Write(reenterDisplayInputStr);
                }

            } while (validInputFlag);

            return input;
        }

        public static DateTime readDate()
        {
            try
            {
                Exception e1 = new Exception("Invalid Input.");
                Exception e2 = new Exception("Date doesn't exist.");
                Exception e3 = new Exception("Date should be greater than or equal to today.");
                System.Console.Write("Date Format(MM/dd/yyyy) :");
                string date = System.Console.ReadLine();
                StringTokenizer st = new StringTokenizer(date, "/");
                if (st.CountTokens() != 3)
                    throw e1;
                int month = int.Parse(st.NextToken());
                int day = int.Parse(st.NextToken());
                int year = int.Parse(st.NextToken());
                if (month < 1 || month > 12)
                    throw e2;
                if (year < DateTime.Now.Year)
                    throw e3;
                if (year == DateTime.Now.Year && month < DateTime.Now.Month)
                    throw e3;

                if (year == DateTime.Now.Year && month == DateTime.Now.Month && day < DateTime.Now.Day)
                    throw e3;
                return new DateTime(year, month, day);

            }
            catch (Exception e)
            {

                System.Console.WriteLine("Invalid Input.");
                throw e;
            }

        }

        public static long[] convertNullableLongArrayToLongArray(long?[] input)
        {
            if (input == null)
            {
                return null;
            }
            long[] output = new long[input.GetLength(0)];
            for (int i = input.GetLength(1); i < input.GetLength(0); i++)
            {
                output[i] = input[i].GetValueOrDefault();
            }
            return output;
        }

        public static long convertNullableLongtoLong(long? input)
        {
            if (input == null)
            {
                return 0;
            }
            long output = input.GetValueOrDefault();
            return output;
        }

    }

}
