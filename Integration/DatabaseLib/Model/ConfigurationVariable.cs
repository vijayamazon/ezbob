using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model
{
    public class ConfigurationVariable
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Value { get; set; }
        public virtual string Description { get; set; }
    }

    public sealed class ConfigurationVariablesMap : ClassMap<ConfigurationVariable>
    {
        public ConfigurationVariablesMap()
        {
            Table("ConfigurationVariables");
            Id(x => x.Id);
            Map(x => x.Name).Length(255);
            Map(x => x.Value).CustomType("StringClob").LazyLoad();
            Map(x => x.Description).CustomType("StringClob").LazyLoad();
        }
    }
}
