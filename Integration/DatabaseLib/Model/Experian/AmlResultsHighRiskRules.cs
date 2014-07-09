namespace EZBob.DatabaseLib.Model.Experian
{
	using FluentNHibernate.Mapping;

	public class AmlResultsHighRiskRules
	{
		public virtual int Id { get; set; }
		public virtual AmlResults AmlResult { get; set; }
		public virtual string RuleId { get; set; }
		public virtual string RuleText { get; set; }
	}

	public sealed class AmlResultsHighRiskRulesMap : ClassMap<AmlResultsHighRiskRules>
	{
		public AmlResultsHighRiskRulesMap()
		{
			Table("AmlResultsHighRiskRules");
			Id(x => x.Id);
			References(x => x.AmlResult, "AmlResultId");
			Map(x => x.RuleId).Length(10);
			Map(x => x.RuleText).Length(500);
		}
	}
}