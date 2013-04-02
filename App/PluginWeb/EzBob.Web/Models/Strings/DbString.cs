using FluentNHibernate.Mapping;

namespace EzBob.Web.Models
{
    public class DbString
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}

namespace EzBob.Web.Models.Mappings
{
    public class DbStringMap : ClassMap<DbString>
    {
        public DbStringMap()
        {
            Table("DbString");
            Cache.Region("DbStrings").ReadWrite();
            Id(x => x.Id).GeneratedBy.Native();
            Not.LazyLoad();
            Map(x => x.Key, "`Key`").Unique();
            Map(x => x.Value, "`Value`");
        }
    }
}