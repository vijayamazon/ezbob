using System.Linq;
using ApplicationMng.Repository;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database
{
    public class MailTemplateRelation
    {
        public virtual int Id { get; set; }
        public virtual string InternalTemplateName { get; set; }
        public virtual MandrillTemplate MandrillTemplate { get; set; }
    }
}

namespace EZBob.DatabaseLib.Model.Database.Mapping
{
    public sealed class MailTemplateRelationMap : ClassMap<MailTemplateRelation>
    {
        public MailTemplateRelationMap()
        {
            Id(x=>x.Id);
            Map(x => x.InternalTemplateName);
            References(x => x.MandrillTemplate,"MandrillTemplateId");
        }
    }
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
    public interface IMailTemplateRelationRepository
    {
        string GetByInternalName(string internalTemplateName);
    }

    public class MailTemplateRelationRepository : NHibernateRepositoryBase<MailTemplateRelation>, IMailTemplateRelationRepository
    {
        public MailTemplateRelationRepository(ISession session) : base(session)
        {
        }

        public string GetByInternalName(string internalTemplateName)
        {
            var templateRelation = GetAll().FirstOrDefault(x => x.InternalTemplateName == internalTemplateName);
            if (templateRelation == null)
            {
                throw new System.NullReferenceException("There is no mandrill template for internal template name '"+internalTemplateName + "'");
            }
            return templateRelation.MandrillTemplate.Name;
        }
    }
}