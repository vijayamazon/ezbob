namespace EZBob.DatabaseLib.Model.Marketplaces.Sage
{
	using FluentNHibernate.Mapping;

	public class MP_SageSalesInvoiceItemMap : ClassMap<MP_SageSalesInvoiceItem>
	{
		public MP_SageSalesInvoiceItemMap()
		{
			Table("MP_SageSalesInvoiceItem");
			Id(x => x.Id);
			References(x => x.Invoice, "InvoiceId");

			Map(x => x.SageId);
			Map(x => x.description).Length(250);
			Map(x => x.quantity);
			Map(x => x.unit_price);
			Map(x => x.net_amount);
			Map(x => x.tax_amount);
			Map(x => x.TaxCodeId);
			Map(x => x.tax_rate_percentage);
			Map(x => x.unit_price_includes_tax);
			Map(x => x.LedgerAccountId);
			Map(x => x.product_code).Length(250);
			Map(x => x.ProductId);
			Map(x => x.ServiceKey);
			Map(x => x.lock_version);
		}
	}
}