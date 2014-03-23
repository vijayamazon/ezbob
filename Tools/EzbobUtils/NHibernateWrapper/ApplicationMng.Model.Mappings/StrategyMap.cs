using FluentNHibernate.Mapping;
using NHibernateWrapper.Email.Accounts;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class StrategyMap : ClassMap<Strategy>
	{
		public StrategyMap()
		{
			base.LazyLoad();
			base.Cache.Region("LongTerm").ReadWrite();
			base.Table("Strategy_Strategy");
			this.Id((Strategy x) => (object)x.Id).Column("StrategyId");
			base.Map((Strategy x) => x.Name).Length(400);
			base.Map((Strategy x) => x.Type).Column("StrategyType").Length(255);
			base.Map((Strategy x) => x.Description).Length(500);
			base.Map((Strategy x) => (object)x.IsDeleted);
			base.Map((Strategy x) => x.DisplayName).Length(350);
			base.Map((Strategy x) => (object)x.CurrentVersionId);
			base.HasManyToMany<AccountBase>((Strategy x) => x.Accounts).AsSet().Table("StrategyAccountRel").ChildKeyColumn("AccountId").ParentKeyColumn("StrategyId").Cascade.All().Cache.ReadWrite().Region("LongTerm");
			base.HasManyToMany<ExportTemplate>((Strategy x) => x.ExportTemplates).AsSet().Table("EXPORT_TEMPLATESTRATREL").ChildKeyColumn("TEMPLATEID").ParentKeyColumn("STRATEGYID").Cascade.All().Cache.ReadWrite().Region("LongTerm");
			base.HasManyToMany<Node>((Strategy x) => x.Nodes).AsSet().Table("Strategy_NodeStrategyRel").ChildKeyColumn("NodeId").ParentKeyColumn("StrategyId").Cascade.All().Cache.ReadWrite().Region("LongTerm");
			base.HasMany<PublicNameStrategy>((Strategy x) => x.PublicNameStrategies).AsSet().KeyColumn("STRATEGYID").Cascade.All().Inverse();
			base.HasManyToMany<CreditProduct>((Strategy x) => x.Products).AsSet().Table("Creditproduct_StrategyRel").ChildKeyColumn("CreditProductId").ParentKeyColumn("StrategyId").Cascade.All();
			base.References<User>((Strategy x) => x.User, "UserId");
			base.Map((Strategy x) => x.SignedDocument, "SignedDocument").CustomType("StringClob").LazyLoad();
			base.Map((Strategy x) => x.SignedDocumentDelete, "SignedDocumentDelete").CustomType("StringClob").LazyLoad();
			base.Map((Strategy x) => x.Xml, "Xml").CustomType("StringClob").LazyLoad();
			base.Map((Strategy x) => (object)x.TerminationDate, "termdate");
			base.Map((Strategy x) => (object)x.CreationDate, "CreationDate");
			base.Map((Strategy x) => (object)x.State);
			base.Map((Strategy x) => x.InDbFormat).Length(2147483647).LazyLoad();
		}
	}
}
