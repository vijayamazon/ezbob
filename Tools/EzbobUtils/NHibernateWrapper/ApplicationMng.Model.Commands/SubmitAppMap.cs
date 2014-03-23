using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Commands
{
	public class SubmitAppMap : SubclassMap<SubmitApp>
	{
		public SubmitAppMap()
		{
			base.Map((SubmitApp x) => x.ParamsXml).CustomType("StringClob");
			base.Map((SubmitApp x) => x.Outlet);
			base.References<AppStatus>((SubmitApp x) => x.Status).Column("`Status`");
		}
	}
}
