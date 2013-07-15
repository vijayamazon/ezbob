using ApplicationMng.Repository;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database
{
    public class MandrillTemplate
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
    }
}

namespace EZBob.DatabaseLib.Model.Database.Mapping
{
    public sealed class MandrillTemplateMap : ClassMap<MandrillTemplate>
    {
        public MandrillTemplateMap()
        {
            Id(x => x.Id);
            Map(x => x.Name);
        }
    }
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
    public interface IMandrillTemplateRepository
    {
    }

    public class MandrillTemplateRepository : NHibernateRepositoryBase<MandrillTemplate>, IMandrillTemplateRepository
    {
        public MandrillTemplateRepository(ISession session)
            : base(session)
        {
        }
    }
}