using System.Xml.Serialization;

namespace EzBob.TeraPeakServiceLib
{
    [XmlRoot( "Statistics" )]
    public class QueryResultStatistic
    {
        public NullableDouble Revenue { get; set; }
        public NullableInt Listings { get; set; }
        public NullableInt Transactions { get; set; }
        public NullableInt Successful { get; set; }
        public NullableInt Bids { get; set; }
        public NullableInt ItemsOffered { get; set; }
        public NullableInt ItemsSold { get; set; }
        public NullableInt AverageSellersPerDay { get; set; }
        public NullableDouble SuccessRate { get; set; }
    }
}