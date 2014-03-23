using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Commands
{
	public class CommandsListMap : ClassMap<CommandsList>
	{
		public CommandsListMap()
		{
			base.Cache.ReadWrite().Region("LongTerm");
			base.Table("CommandsList");
			this.Id((CommandsList x) => x.Id).GeneratedBy.UuidHex("");
			base.HasMany<CommandBase>((CommandsList x) => x.Commands).AsList(delegate(ListIndexPart i)
			{
				i.Column("Position");
			}).KeyColumn("ListId").Cascade.All().Cache.ReadWrite().Region("LongTerm");
			base.References<SecurityApplication>((CommandsList x) => x.SecApp).Column("SecAppId");
			base.References<Application>((CommandsList x) => x.App).Column("AppId");
			base.References<User>((CommandsList x) => x.User).Column("UserId");
			base.Map((CommandsList x) => x.Description);
			base.Map((CommandsList x) => (object)x.CreatedOn);
		}
	}
}
