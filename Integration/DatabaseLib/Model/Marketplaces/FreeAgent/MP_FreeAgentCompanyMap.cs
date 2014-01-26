namespace EZBob.DatabaseLib.Model.Marketplaces.FreeAgent
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class MP_FreeAgentCompanyMap : ClassMap<MP_FreeAgentCompany>
	{
		public MP_FreeAgentCompanyMap()
		{
			Table("MP_FreeAgentCompany");
			Id(x => x.Id);
			References(x => x.Request, "RequestId");

			Map(x => x.url).Length(250);
			Map(x => x.name).Length(250);
			Map(x => x.subdomain).Length(250);
			Map(x => x.type).Length(250);
			Map(x => x.currency).Length(250);
			Map(x => x.mileage_units).Length(250);
			Map(x => x.company_start_date).CustomType<UtcDateTimeType>();
			Map(x => x.freeagent_start_date).CustomType<UtcDateTimeType>();
			Map(x => x.first_accounting_year_end).Length(250);
			Map(x => x.company_registration_number).Length(20);
			Map(x => x.sales_tax_registration_status).Length(250);
			Map(x => x.sales_tax_registration_number).Length(20);
		}
	}
}