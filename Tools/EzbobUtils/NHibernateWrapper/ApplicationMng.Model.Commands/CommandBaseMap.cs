using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Commands
{
	public class CommandBaseMap : ClassMap<CommandBase>
	{
		public CommandBaseMap()
		{
			base.Cache.ReadWrite().Region("LongTerm");
			base.Table("Commands");
			this.Id((CommandBase x) => (object)x.Id).GeneratedBy.HiLo("1000");
			this.DiscriminateSubClassesOnColumn<string>("`Type`");
			base.References<Application>((CommandBase x) => x.App).Column("AppId");
			base.References<SecurityApplication>((CommandBase x) => x.SecApp).Column("SecAppId");
			base.Map((CommandBase x) => (object)x.Position);
			base.Map((CommandBase x) => (object)x.SignatureRequired).Not.Nullable();
			base.Map((CommandBase x) => x.ItemsToBeSigned);
			base.Map((CommandBase x) => x.OutletName);
			base.Map((CommandBase x) => x.NodeName);
		}
	}
}
