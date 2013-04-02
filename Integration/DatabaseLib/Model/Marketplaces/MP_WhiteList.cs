using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public class MP_WhiteList
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual System.Guid MarketplaceType { get; set; }
    }

    public class MP_WhiteListMap : ClassMap<MP_WhiteList>
    {
        public MP_WhiteListMap()
        {
            Table("MP_WhiteList");
            Id(x => x.Id).GeneratedBy.Native();
            Map(x => x.Name).Length(500);
            Map(x => x.MarketplaceType, "MarketPlaceTypeGuid");
        }
    }
}