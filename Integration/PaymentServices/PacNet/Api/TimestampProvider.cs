using System;

namespace Raven.API.Support
{
    /**
     * <p>
     * Supplies timestamp strings for the current time, suitable for use by Raven
     * using the format "2006-01-17T15:26:30.Z".
     * </p>
     *
     * @author warren
     */
    public class TimestampProvider
    {

        /**
         * <p>
         * Answers a timestamp string for the current time, suitable for use by
         * Raven using the format "2006-01-17T15:26:30.000Z".
         * </p>
         *
         * @return a timestamp string for the current time with format "2006-01-17T15:26:30.000Z"
         */
        public string GetFormattedTimestamp()
        {
            return (((DateTime.Now).ToUniversalTime()).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"));
        }

        /**
         * <p>
         * Answers a timestamp string for the supplied time, suitable for use by
         * Raven using the format "2006-01-17T15:26:30.000Z".
         * </p>
         *
         * @return a timestamp string for the supplied time with format "2006-01-17T15:26:30.000Z"
         */
        public string FormatTimestamp(DateTime aDateTime)
        {
            return aDateTime.ToUniversalTime().ToString("s");
        }

    }
}
