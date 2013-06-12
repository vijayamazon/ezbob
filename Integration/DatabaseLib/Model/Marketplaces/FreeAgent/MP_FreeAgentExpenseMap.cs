namespace EZBob.DatabaseLib.Model.Marketplaces.FreeAgent
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class MP_FreeAgentExpenseMap : ClassMap<MP_FreeAgentExpense>
	{
		public MP_FreeAgentExpenseMap()
		{
			Table("MP_FreeAgentExpense");
			Id(x => x.Id);
			References(x => x.Request, "RequestId");

			Map(x => x.url).Length(250);
			Map(x => x.username).Length(250);
			Map(x => x.category).Length(250);
			Map(x => x.dated_on).CustomType<UtcDateTimeType>();
			Map(x => x.currency).Length(10);
			Map(x => x.gross_value);
			Map(x => x.native_gross_value);
			Map(x => x.sales_tax_rate);
			Map(x => x.sales_tax_value);
			Map(x => x.native_sales_tax_value);
			Map(x => x.description).Length(250);
			Map(x => x.manual_sales_tax_amount);
			Map(x => x.updated_at).CustomType<UtcDateTimeType>();
			Map(x => x.created_at).CustomType<UtcDateTimeType>();

			Map(x => x.attachment_url).Length(250);
			Map(x => x.attachment_content_src).Length(1000);
			Map(x => x.attachment_content_type).Length(250);
			Map(x => x.attachment_file_name).Length(250);
			Map(x => x.attachment_file_size);
			Map(x => x.attachment_description).Length(250);
		}
	}
}