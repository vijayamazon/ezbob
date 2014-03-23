using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class NodeMap : ClassMap<Node>
	{
		public NodeMap()
		{
			base.LazyLoad();
			base.Cache.ReadWrite().Region("LongTerm");
			base.Table("Strategy_Node");
			this.Id((Node x) => (object)x.Id).GeneratedBy.Native().Column("NodeId");
			base.Map((Node x) => x.Name).Length(150);
			base.Map((Node x) => x.DisplayName).Length(255);
			base.Map((Node x) => x.Description).Length(500);
			base.Map((Node x) => (object)x.IsDeleted);
			base.References<SecurityApplication>((Node x) => x.SecApp).Column("ApplicationId");
			base.Map((Node x) => (object)x.IsHardReaction);
			base.Map((Node x) => (object)x.ContainsPrint);
			base.Map((Node x) => x.Guid).Length(256);
			base.Map((Node x) => (object)x.ExecutionDuration);
			base.Map((Node x) => (object)x.TerminationDate);
			base.References<User>((Node x) => x.User).Column("CreatorUserId").LazyLoad();
			base.Map((Node x) => (object)x.StartDate);
			base.Map((Node x) => x.SignedDocument).CustomType("StringClob").LazyLoad();
			base.Map((Node x) => x.SignedDocumentDelete).CustomType("StringClob").LazyLoad();
			base.Map((Node x) => x.Ndx).Length(2147483647).LazyLoad();
		}
	}
}
