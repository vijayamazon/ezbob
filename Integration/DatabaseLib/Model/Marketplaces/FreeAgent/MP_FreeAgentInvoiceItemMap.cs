namespace EZBob.DatabaseLib.Model.Marketplaces.FreeAgent
{
	using FluentNHibernate.Mapping;

	public class MP_FreeAgentInvoiceItemMap : ClassMap<MP_FreeAgentInvoiceItem>
	{
		public MP_FreeAgentInvoiceItemMap()
		{
			Table("MP_FreeAgentInvoiceItem");
			Id(x => x.Id);
			References(x => x.Invoice, "InvoiceId");

			Map(x => x.url).Length(250);
			Map(x => x.position);
			Map(x => x.description).Length(250);
			Map(x => x.item_type).Length(250);
			Map(x => x.price);
			Map(x => x.quantity);
			Map(x => x.category).Length(250);
		}
	}
}