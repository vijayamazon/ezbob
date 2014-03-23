using FluentNHibernate.Mapping;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace ApplicationMng.Model
{
	public class BusinessEntitiyToNodeRelationMap : ClassMap<BusinessEntitiyToNodeRelation>
	{
		public BusinessEntitiyToNodeRelationMap()
		{
			base.Table("BusinessEntity_NodeRel");
			this.Id((BusinessEntitiyToNodeRelation x) => (object)x.Id).Column("Id").GeneratedBy.HiLo("1");
			base.References<Node>((BusinessEntitiyToNodeRelation x) => x.Node, "NodeId");
			base.References<BusinessEntity>((BusinessEntitiyToNodeRelation x) => x.BusinessEntity, "BusinessEntityId");
		}
	}
}
