using FluentNHibernate.Mapping;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace ApplicationMng.Model
{
	public class BusinessEntitiyToStrategyRelationMap : ClassMap<BusinessEntitiyToStrategyRelation>
	{
		public BusinessEntitiyToStrategyRelationMap()
		{
			base.Table("BusinessEntity_StrategyRel");
			this.Id((BusinessEntitiyToStrategyRelation x) => (object)x.Id).Column("Id").GeneratedBy.HiLo("1");
			base.References<Strategy>((BusinessEntitiyToStrategyRelation x) => x.Strategy, "StrategyId");
			base.References<BusinessEntity>((BusinessEntitiyToStrategyRelation x) => x.BusinessEntity, "BusinessEntityId");
		}
	}
}
