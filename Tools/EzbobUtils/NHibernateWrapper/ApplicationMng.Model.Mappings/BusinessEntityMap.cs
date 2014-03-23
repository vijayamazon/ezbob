using FluentNHibernate.Mapping;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class BusinessEntityMap : ClassMap<BusinessEntity>
	{
		public BusinessEntityMap()
		{
			base.Cache.Region("LongTerm").ReadWrite();
			base.Table("BusinessEntity");
			this.Id((BusinessEntity x) => (object)x.Id).GeneratedBy.HiLo("1");
			base.Map((BusinessEntity x) => x.Name).Length(25);
			base.Map((BusinessEntity x) => x.Description).Length(1024);
			base.Map((BusinessEntity x) => x.Comment).Column("Review").Length(1024);
			base.Map((BusinessEntity x) => (object)x.IsDeleted);
			base.Map((BusinessEntity x) => (object)x.CreationDate);
			base.Map((BusinessEntity x) => (object)x.TerminationDate);
			base.Map((BusinessEntity x) => x.Version).Column("ItemVersion").Length(50);
			base.References<User>((BusinessEntity x) => x.User, "UserId");
			base.Map((BusinessEntity x) => x.Document).CustomType("StringClob").LazyLoad();
			base.Map((BusinessEntity x) => x.SignedDocument, "SignedDocument").CustomType("StringClob").LazyLoad();
			base.Map((BusinessEntity x) => x.SignedDocumentDelete, "SignedDocumentDelete").CustomType("StringClob").LazyLoad();
		}
	}
}
