using System;
using System.Globalization;
using System.Xml.Serialization;

namespace EzBob.TeraPeakServiceLib
{
    [XmlRoot( "Statistics" )]
    public class QueryResultStatistic
    {

        private double? TryParseDouble(string val)
        {
            if (string.IsNullOrEmpty(val)) return null;
            return Convert.ToDouble(val, CultureInfo.InvariantCulture.NumberFormat);
        }

        private int? TryParseInt(string val)
        {
            if (string.IsNullOrEmpty(val)) return null;
            return Convert.ToInt32(val, CultureInfo.InvariantCulture.NumberFormat);
        }

        public double? Revenue { get; set; }
        public int? Listings { get; set; }
        public int? Transactions { get; set; }
        public int? Successful { get; set; }
        public int? Bids { get; set; }
        public int? ItemsOffered { get; set; }
        public int? ItemsSold { get; set; }
        public int? AverageSellersPerDay { get; set; }
        
        [XmlIgnore]
        public double? SuccessRate { get; set; }
        
        [XmlElement("SuccessRate")]
        public string SuccessRateString
        {
            get { return SuccessRate.ToString(); }
            set { SuccessRate = TryParseDouble(value); }
        }
    }
}