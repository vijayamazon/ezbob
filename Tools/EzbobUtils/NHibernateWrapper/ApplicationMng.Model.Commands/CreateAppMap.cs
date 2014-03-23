using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Commands
{
	public class CreateAppMap : SubclassMap<CreateApp>
	{
		public CreateAppMap()
		{
			base.Map((CreateApp x) => x.StrategyName).Column("StrategyName");
		}
	}
}
