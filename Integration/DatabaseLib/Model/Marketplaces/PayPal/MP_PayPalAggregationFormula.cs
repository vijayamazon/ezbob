namespace EZBob.DatabaseLib.Model.Marketplaces.PayPal
{
	public class MP_PayPalAggregationFormula
	{
		public virtual int Id { get; set; }
		public virtual int FormulaNum { get; set; }
		public virtual string FormulaName { get; set; }
		public virtual string Type { get; set; }
		public virtual string Status { get; set; }
		public virtual bool? Positive { get; set; }
	}
}