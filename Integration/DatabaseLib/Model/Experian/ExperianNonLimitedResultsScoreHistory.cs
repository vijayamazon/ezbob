namespace EZBob.DatabaseLib.Model.Experian
{
	using System;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class ExperianNonLimitedResultsScoreHistory
	{
		public virtual int Id { get; set; }
		public virtual ExperianNonLimitedResults ExperianNonLimitedResult { get; set; }
		public virtual int RiskScore { get; set; }
		public virtual DateTime Date { get; set; }
	}

	public sealed class ExperianNonLimitedResultsScoreHistoryMap : ClassMap<ExperianNonLimitedResultsScoreHistory>
	{
		public ExperianNonLimitedResultsScoreHistoryMap()
		{
			Table("ExperianNonLimitedResultsScoreHistory");
			Id(x => x.Id);
			References(x => x.ExperianNonLimitedResult, "NonLimitedResultId");
			Map(x => x.RiskScore);
			Map(x => x.Date).Access.BackingField().CustomType<UtcDateTimeType>();
		}
	}
}