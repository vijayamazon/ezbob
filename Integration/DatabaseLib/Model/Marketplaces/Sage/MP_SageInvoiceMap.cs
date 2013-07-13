namespace EZBob.DatabaseLib.Model.Marketplaces.Sage
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class MP_SageInvoiceMap : ClassMap<MP_SageInvoice>
	{
		public MP_SageInvoiceMap()
		{
			Table("MP_SageInvoice");
			Id(x => x.Id);
			References(x => x.Request, "RequestId");

			HasMany(x => x.Items).KeyColumn("InvoiceId").Cascade.All();

			Map(x => x.SageId);
			Map(x => x.invoice_number).Length(250);
			Map(x => x.StatusId);
			Map(x => x.due_date).CustomType<UtcDateTimeType>();
			Map(x => x.date).CustomType<UtcDateTimeType>();
			Map(x => x.void_reason).Length(250);
			Map(x => x.outstanding_amount);
			Map(x => x.total_net_amount);
			Map(x => x.total_tax_amount);
			Map(x => x.tax_scheme_period_id);
			Map(x => x.carriage);
			Map(x => x.CarriageTaxCodeId);
			Map(x => x.carriage_tax_rate_percentage);
			Map(x => x.ContactId);
			Map(x => x.contact_name).Length(250);
			Map(x => x.main_address).Length(250);
			Map(x => x.delivery_address).Length(250);
			Map(x => x.delivery_address_same_as_main);
			Map(x => x.reference).Length(250);
			Map(x => x.notes).Length(250);
			Map(x => x.terms_and_conditions).Length(250);
			Map(x => x.lock_version);
		}
	}
}