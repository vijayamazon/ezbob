namespace EZBob.DatabaseLib.Model.CustomerRelations
{
	using FluentNHibernate.Mapping;

	public class CRMStatusesMap : ClassMap<CRMStatuses>
	{
		public CRMStatusesMap()
		{
			Table("CRMStatuses");
			Id(x => x.Id);

			Map(x => x.Name).Length(100);
			Map(x => x.GroupId);
		}
	}
}