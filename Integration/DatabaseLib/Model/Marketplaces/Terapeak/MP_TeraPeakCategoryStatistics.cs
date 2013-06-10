using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public class MP_TeraPeakCategoryStatistics
    {
        public virtual int Id { get; set; }
        public virtual int Listings { get; set; }
        public virtual int Successful { get; set; }
        public virtual int ItemsSold { get; set; }
        public virtual double Revenue { get; set; }
        public virtual double SuccessRate { get; set; }
        //note OrderItem is actuall not an order item. It's a aggregated data for some time period.
        public virtual MP_TeraPeakOrderItem OrderItem { get; set; }
        public virtual MP_TeraPeakCategory Category { get; set; }
    }

    public class MP_TeraPeakCategoryStatisticsMap : ClassMap<MP_TeraPeakCategoryStatistics>
    {
        public MP_TeraPeakCategoryStatisticsMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("100");
            Map(x => x.Listings);
            Map(x => x.Successful);
            Map(x => x.ItemsSold);
            Map(x => x.Revenue);
            Map(x => x.SuccessRate);
            References(x => x.OrderItem, "OrderItemId");
            References(x => x.Category, "CategoryId");
        }
    }
}