using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Commands
{
	public class SaveAppMap : SubclassMap<SaveApp>
	{
		public SaveAppMap()
		{
			base.Map((SaveApp x) => x.ParamsXml).CustomType("StringClob");
			base.References<AppStatus>((SaveApp x) => x.Status).Column("`Status`");
			base.Map((SaveApp x) => x.ControlName);
			base.Map((SaveApp x) => x.FormName);
		}
	}
}
