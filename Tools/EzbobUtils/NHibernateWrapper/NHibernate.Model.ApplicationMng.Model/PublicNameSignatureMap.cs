using ApplicationMng.Model;
using FluentNHibernate.Mapping;
using System;
namespace NHibernateWrapper.NHibernate.Model.ApplicationMng.Model
{
	public class PublicNameSignatureMap : ClassMap<PublicNameSignature>
	{
		public PublicNameSignatureMap()
		{
			base.LazyLoad();
			base.Table("Strategy_PublicSign");
			this.Id((PublicNameSignature x) => (object)x.Id).GeneratedBy.Native();
			base.References<User>((PublicNameSignature x) => x.User, "UserId");
			base.Map((PublicNameSignature x) => (object)x.ChangeDate, "CreationDate");
			base.Map((PublicNameSignature x) => x.Action);
			base.Map((PublicNameSignature x) => x.SignedDocument).CustomType("StringClob").LazyLoad();
			base.Map((PublicNameSignature x) => x.Data).LazyLoad();
			base.Map((PublicNameSignature x) => x.AllData);
			base.References<PublicName>((PublicNameSignature x) => x.PublicName, "StrategyPublicId");
		}
	}
}
