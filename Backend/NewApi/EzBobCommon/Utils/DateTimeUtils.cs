namespace EzBobCommon.Utils {
    using System;
    using System.Globalization;

    public class DateTimeUtils {
        /// <summary>
        /// Parses the date of birth.
        /// </summary>
        /// <param name="dateOfBirth">The date of birth.</param>
        /// <returns></returns>
        public static DateTime ParseDateOfBirth(string dateOfBirth) {
            return DateTime.ParseExact(dateOfBirth, "d/M/yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts to iso 8601 format.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static string ConvertToIso8601String(DateTime dateTime) {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }
    }
}
