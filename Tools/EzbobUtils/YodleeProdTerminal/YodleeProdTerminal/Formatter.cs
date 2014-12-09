using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.yodlee.sampleapps
{
    public class Formatter
    {
        public static String DATE_SHORT_FORMAT = "{0:d}"; //"MM-dd-yyyy";
        public static String DATE_LONG_FORMAT = "{0:G}";  //"MM-dd-yyyy hh:mm:ss";

        /**
         * Format date using the format specified in the parameter.
         * @param date
         * @param dateFormat
         * @return
         */
        public static String formatDate(DateTime? date, String dateFormat)
        {
            if(date == null ){
                return "";
            }
            return String.Format(dateFormat, date);
         }

        /**
         * Format a number but adding commas
         *
         * @param number
         * @return number with commas
         */
        /**public static String formatNumber(long number){
            NumberFormat  nf = new DecimalFormat("#,###,###");
            return nf.format(number);
        }*/

        /**
         * Format a number but adding commas
         *
         * @param number
         * @return number with commas
         */
        /**public static String formatNumber(Long number){
            return formatNumber(number.longValue()) ;
        }*/

        /**
         * Format a number but adding commas
         *
         * @param number
         * @return number with commas
         */
        /**public static String formatNumber(Double number){
            return formatNumber(number.longValue()) ;
        }*/

        /**
         * Format a number but adding commas
         *
         * @param money
         * @return number with commas and currenct code
         */
        public static String formatMoney(YMoney money){
            if(money == null){
                return "null";
            }
            /**if(money.amount == null){
                return "null";
            }*/
            String m = String.Format("{0:N2}", money.amount);
            //NumberFormat  nf = new DecimalFormat("#,###,###.##");
            if(money.currencyCode.Equals("USD")){
                m = "$"+m;
            } else{
                m = m + " " + money.currencyCode ;
            }
            return m;
        }        
    }
}
