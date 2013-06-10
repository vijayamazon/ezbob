namespace EZBob.DatabaseLib.Model.Marketplaces.FreeAgent
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class MP_FreeAgentInvoiceMap : ClassMap<MP_FreeAgentInvoice>
	{
		public MP_FreeAgentInvoiceMap()
		{
			Table("MP_FreeAgentInvoice");
			Id(x => x.Id);
			References(x => x.Request, "RequestId");

			HasMany(x => x.Items).KeyColumn("InvoiceId").Cascade.All();

			Map(x => x.url).Length(250);
			Map(x => x.contact).Length(250);
			Map(x => x.dated_on).CustomType<UtcDateTimeType>();
			Map(x => x.due_on).CustomType<UtcDateTimeType>();
			Map(x => x.reference).Length(250);
			Map(x => x.currency).Length(10);
			Map(x => x.exchange_rate);
			Map(x => x.net_value);
			Map(x => x.total_value);
			Map(x => x.paid_value);
			Map(x => x.due_value);
			Map(x => x.status).Length(250);
			Map(x => x.omit_header);
			Map(x => x.payment_terms_in_days);
			Map(x => x.paid_on).CustomType<UtcDateTimeType>();
		}
	}
}