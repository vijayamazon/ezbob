using System.Xml.Serialization;

namespace EzBob.TeraPeakServiceLib
{
    [XmlRoot( "Statistics" )]
    public class QueryResultStatistic
    {
        public double? Revenue { get; set; }
        public int? Listings { get; set; }
        public int? Transactions { get; set; }
        public int? Successful { get; set; }
        public int? Bids { get; set; }
        public int? ItemsOffered { get; set; }
        public int? ItemsSold { get; set; }
        public int? AverageSellersPerDay { get; set; }
        public double? SuccessRate { get; set; }
    }
}