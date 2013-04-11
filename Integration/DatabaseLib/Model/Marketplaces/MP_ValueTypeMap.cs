using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public class MP_ValueTypeMap : ClassMap<MP_ValueType>
    {
        public MP_ValueTypeMap()
        {
            Table("MP_ValueType");
            Cache.Region("LongTerm").ReadWrite();
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.Name).Not.Nullable().Length(50);
            Map(x => x.InternalId).Not.Nullable();
            Map(x => x.Description);
            HasMany(x => x.AnalyisisFunctions).KeyColumn("ValueTypeId");
        }
    }
}