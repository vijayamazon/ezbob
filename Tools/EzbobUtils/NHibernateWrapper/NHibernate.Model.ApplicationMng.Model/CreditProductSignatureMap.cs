using ApplicationMng.Model;
using FluentNHibernate.Mapping;
using System;
namespace NHibernateWrapper.NHibernate.Model.ApplicationMng.Model
{
	public class CreditProductSignatureMap : ClassMap<CreditProductSignature>
	{
		public CreditProductSignatureMap()
		{
			base.LazyLoad();
			base.Table("CreditProduct_Sign");
			this.Id((CreditProductSignature x) => (object)x.Id).GeneratedBy.Native();
			base.References<User>((CreditProductSignature x) => x.User, "UserId");
			base.Map((CreditProductSignature x) => (object)x.ChangeDate, "CreationDate");
			base.Map((CreditProductSignature x) => x.SignedDocument).CustomType("StringClob").LazyLoad();
			base.Map((CreditProductSignature x) => x.Data).LazyLoad();
			base.References<CreditProduct>((CreditProductSignature x) => x.CreditProduct, "CreditProductId");
		}
	}
}
