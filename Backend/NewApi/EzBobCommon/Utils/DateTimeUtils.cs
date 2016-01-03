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
    }
}
