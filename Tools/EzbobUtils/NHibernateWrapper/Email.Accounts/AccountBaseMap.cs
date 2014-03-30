using ApplicationMng.Model;
using FluentNHibernate.Mapping;
using System;
namespace NHibernateWrapper.Email.Accounts
{
	public class AccountBaseMap : ClassMap<AccountBase>
	{
		public AccountBaseMap()
		{
			base.Table("EmailAccount");
			this.Id((AccountBase x) => (object)x.Id).GeneratedBy.HiLo("1000");
			base.Map((AccountBase x) => x.Name).Length(1024);
			base.Map((AccountBase x) => x.Description).Length(1024);
			base.Map((AccountBase x) => x.EmailFrom).Length(1024);
			base.Map((AccountBase x) => (object)x.StartDate);
			base.Map((AccountBase x) => (object)x.IsDeleted);
			base.References<User>((AccountBase x) => x.User).Column("CreatorUserId").LazyLoad();
			base.Map((AccountBase x) => x.SignedDocument).CustomType("StringClob");
			base.Map((AccountBase x) => x.SignedDocumentDelete).CustomType("StringClob");
			base.Map((AccountBase x) => (object)x.TerminationDate);
			this.DiscriminateSubClassesOnColumn<string>("Type");
		}
	}
}
