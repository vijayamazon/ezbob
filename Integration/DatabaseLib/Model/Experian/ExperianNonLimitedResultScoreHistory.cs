namespace EZBob.DatabaseLib.Model.Experian
{
	using System;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class ExperianNonLimitedResultScoreHistory
	{
		public virtual int Id { get; set; }
		public virtual ExperianNonLimitedResults ExperianNonLimitedResult { get; set; }
		public virtual int RiskScore { get; set; }
		public virtual DateTime Date { get; set; }
	}

	public sealed class ExperianNonLimitedResultScoreHistoryMap : ClassMap<ExperianNonLimitedResultScoreHistory>
	{
		public ExperianNonLimitedResultScoreHistoryMap()
		{
			Table("ExperianNonLimitedResultScoreHistory");
			Id(x => x.Id);
			References(x => x.ExperianNonLimitedResult, "ExperianNonLimitedResultId");
			Map(x => x.RiskScore);
			Map(x => x.Date).Access.BackingField().CustomType<UtcDateTimeType>();
		}
	}
}