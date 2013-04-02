using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public class CaisFlagMap : ClassMap<CaisFlag>
    {
        public CaisFlagMap()
        {
            Table("CaisFlags");
            Id(x => x.Id).GeneratedBy.Native().Column("Id");
            Map(x => x.Description);
            Map(x => x.FlagSetting);
            Map(x => x.ValidForRecordType);
            Map(x => x.Comment);

        }
    }
}
