namespace EZBob.DatabaseLib.Model.CustomerRelations
{
	using FluentNHibernate.Mapping;

	public class CRMActionsMap : ClassMap<CRMActions>
	{
		public CRMActionsMap()
		{
			Table("CRMActions");
			Id(x => x.Id);

			Map(x => x.Name).Length(100);
		}
	}
}