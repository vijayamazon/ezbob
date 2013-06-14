namespace EZBob.DatabaseLib.Model.Marketplaces.FreeAgent
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class MP_FreeAgentExpenseCategoryMap : ClassMap<MP_FreeAgentExpenseCategory>
	{
		public MP_FreeAgentExpenseCategoryMap()
		{
			Table("MP_FreeAgentExpenseCategory");
			Id(x => x.Id);

			Map(x => x.category_group).Length(250);
			Map(x => x.url).Length(250);
			Map(x => x.description).Length(250);
			Map(x => x.nominal_code).Length(250);
			Map(x => x.allowable_for_tax);
			Map(x => x.tax_reporting_name).Length(250);
			Map(x => x.auto_sales_tax_rate).Length(250);
		}
	}
}