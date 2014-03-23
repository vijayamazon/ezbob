using FluentNHibernate.Mapping;
using System;
namespace NHibernateWrapper.NHibernate.Model.Commands
{
	[System.Serializable]
	public class SaveAutonodeDataMap : SubclassMap<SaveAutonodeData>
	{
		public SaveAutonodeDataMap()
		{
			base.Map((SaveAutonodeData x) => x.Outparams).CustomType("StringClob");
			base.Map((SaveAutonodeData x) => x.Outlet);
		}
	}
}
