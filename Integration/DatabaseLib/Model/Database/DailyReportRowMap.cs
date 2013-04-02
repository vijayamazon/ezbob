using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public class DailyReportRowMap : ClassMap<DailyReportRow>
    {
        public DailyReportRowMap()
        {
            Id(x => x.Id);
            Map(x => x.Date);
            Map(x => x.CustomerName);
            Map(x => x.Expected);
            Map(x => x.LoanAmount);
            Map(x => x.LoanBalance);
            Map(x => x.LoanRef);
            Map(x => x.OriginationDate);
            Map(x => x.Paid);
            Map(x => x.Status);
        }
    }
}
