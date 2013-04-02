using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
    public class CaisReportsHistoryMap : ClassMap<CaisReportsHistory>
    {
        public CaisReportsHistoryMap()
        {
            Table("CaisReportsHistory");
            Id(x => x.Id).GeneratedBy.Native().Column("Id");
            Map(x => x.Date).CustomType<UtcDateTimeType>(); 
            Map(x => x.FileName).CustomType<CaisReportType>();
            Map(x => x.Type);
            Map(x => x.OfItems);
            Map(x => x.GoodUsers);
            Map(x => x.UploadStatus).CustomType<CaisReportUploadStatus>();
            Map(x => x.FilePath);
        }
    }
}
