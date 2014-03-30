using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Mappings
{
	public sealed class CreditProductMap : ClassMap<CreditProduct>
	{
		public CreditProductMap()
		{
			this.Id((CreditProduct x) => (object)x.Id);
			base.Cache.ReadWrite().Region("LongTerm");
			base.Table("CreditProduct_Products");
			base.Map((CreditProduct x) => x.Name).Length(1024);
			base.Map((CreditProduct x) => x.Description).Length(1024);
			base.Map((CreditProduct x) => (object)x.CreationDate);
			base.References<User>((CreditProduct x) => x.User).Column("UserId");
			base.Map((CreditProduct x) => (object)x.IsDeleted);
			base.HasMany<CreditProductParam>((CreditProduct x) => x.Params).KeyColumn("CreditProductId").Cascade.All().Inverse().Cache.ReadWrite().Region("LongTerm");
		}
	}
}
