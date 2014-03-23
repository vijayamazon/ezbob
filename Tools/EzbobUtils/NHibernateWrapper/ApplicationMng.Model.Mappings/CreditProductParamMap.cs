using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class CreditProductParamMap : ClassMap<CreditProductParam>
	{
		public CreditProductParamMap()
		{
			this.Id((CreditProductParam x) => (object)x.Id);
			base.Cache.ReadWrite().Region("LongTerm");
			base.Table("CreditProduct_Params");
			base.Map((CreditProductParam x) => x.Name).Length(1024);
			base.Map((CreditProductParam x) => x.Type).Length(1024);
			base.Map((CreditProductParam x) => x.Description).Length(1024);
			base.References<CreditProduct>((CreditProductParam x) => x.Product).Column("CreditProductId");
			base.Map((CreditProductParam x) => x.Value).Length(2096);
		}
	}
}
