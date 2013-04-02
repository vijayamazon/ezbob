using FluentNHibernate.Mapping;

namespace EzBob.Web.Models
{
    public class AllowedEmail
    {
        public virtual int Id { get; set; }
        public virtual string Email { get; set; }
    }

    public class AllowedEmailMap : ClassMap<AllowedEmail>
    {
        public AllowedEmailMap()
        {
            Table("AllowedEmail");
            Id(x => x.Id).GeneratedBy.Native();
            Map(x => x.Email, "AllowedEmail").Length(250);
            Cache.Region("LongTerm").ReadWrite();
        }
    }
}