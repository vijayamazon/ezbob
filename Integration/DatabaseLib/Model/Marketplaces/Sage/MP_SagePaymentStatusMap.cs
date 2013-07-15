namespace EZBob.DatabaseLib.Model.Marketplaces.Sage
{
	using FluentNHibernate.Mapping;

	public class MP_SagePaymentStatusMap : ClassMap<MP_SagePaymentStatus>
	{
		public MP_SagePaymentStatusMap()
		{
			Table("MP_SagePaymentStatus");
			Id(x => x.Id);
			Map(x => x.SageId);
			Map(x => x.name);
		}
	}
}