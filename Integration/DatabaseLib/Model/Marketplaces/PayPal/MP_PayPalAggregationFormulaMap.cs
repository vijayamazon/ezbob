namespace EZBob.DatabaseLib.Model.Marketplaces.PayPal
{
	using FluentNHibernate.Mapping;

	public class MP_PayPalAggregationFormulaMap : ClassMap<MP_PayPalAggregationFormula>
	{
		public MP_PayPalAggregationFormulaMap()
		{
			Table("MP_PayPalAggregationFormula");
			Id( x => x.Id );
			Map(x => x.FormulaNum);
			Map(x => x.FormulaName);
			Map(x => x.Type);
			Map(x => x.Status);
			Map(x => x.Positive);
		}
	}
}