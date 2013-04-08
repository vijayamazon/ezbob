using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
    public sealed class CaisReportsHistoryMap : ClassMap<CaisReportsHistory>
    {
        public CaisReportsHistoryMap()
        {
            Table("CaisReportsHistory");
            Id(x => x.Id).GeneratedBy.Native().Column("Id");
            Map(x => x.Date).CustomType<UtcDateTimeType>();
            Map(x => x.FileName);
            Map(x => x.Type).CustomType<CaisType>();
            Map(x => x.OfItems);
            Map(x => x.GoodUsers);
            Map(x => x.UploadStatus).CustomType<CaisUploadStatus>();
            Map(x => x.FilePath);
            Map(x => x.Defaults);
        }
    }
}