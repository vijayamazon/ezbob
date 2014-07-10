namespace EZBob.DatabaseLib.Model.Experian
{
	using System;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using Iesi.Collections.Generic;
	using NHibernate;
	using NHibernate.Type;

	public class ExperianNonLimitedResults
	{
		public virtual int Id { get; set; }
		public virtual int CustomerId { get; set; }
		public virtual string RefNumber { get; set; }
		public virtual long ServiceLogId { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual string BusinessName { get; set; }
		public virtual string Address1 { get; set; }
		public virtual string Address2 { get; set; }
		public virtual string Address3 { get; set; }
		public virtual string Address4 { get; set; }
		public virtual string Address5 { get; set; }
		public virtual string Postcode { get; set; }
		public virtual DateTime? IncorporationDate { get; set; }
		public virtual int RiskScore { get; set; }
		public virtual int Score { get; set; }
		public virtual int CreditLimit { get; set; }
		public virtual int AgeOfMostRecentCcj { get; set; }
		public virtual int NumOfCcjsInLast12Months { get; set; }
		public virtual int NumOfCcjsIn13To24Months { get; set; }
		public virtual int SumOfCcjsInLast12Months { get; set; }
		public virtual int SumOfCcjsIn13To24Months { get; set; }
		public virtual int NumOfCcjsInLast24Months { get; set; }
		public virtual int NumOfAssociatedCcjsInLast24Months { get; set; }
		public virtual int SumOfCcjsInLast24Months { get; set; }
		public virtual int SumOfAssociatedCcjsInLast24Months { get; set; }
		public virtual bool IsActive { get; set; }

		public virtual ISet<ExperianNonLimitedResultsScoreHistory> HistoryScores { get; set; }

		public ExperianNonLimitedResults()
		{
			HistoryScores = new HashedSet<ExperianNonLimitedResultsScoreHistory>();
		}
	}

	public sealed class ExperianNonLimitedResultsMap : ClassMap<ExperianNonLimitedResults>
	{
		public ExperianNonLimitedResultsMap()
		{
			Table("ExperianNonLimitedResults");
			Id(x => x.Id);
			Map(x => x.CustomerId);
			Map(x => x.RefNumber).Length(50);
			Map(x => x.ServiceLogId);
			Map(x => x.Created).CustomType<UtcDateTimeType>().Not.Nullable();
			Map(x => x.BusinessName).Length(75);
			Map(x => x.Address1).Length(30);
			Map(x => x.Address2).Length(30);
			Map(x => x.Address3).Length(30);
			Map(x => x.Address4).Length(30);
			Map(x => x.Address5).Length(30);
			Map(x => x.Postcode).Length(8);
			Map(x => x.IncorporationDate).CustomType<UtcDateTimeType>();
			Map(x => x.RiskScore);
			Map(x => x.Score);
			Map(x => x.CreditLimit);
			Map(x => x.AgeOfMostRecentCcj);
			Map(x => x.NumOfCcjsInLast12Months);
			Map(x => x.NumOfCcjsIn13To24Months);
			Map(x => x.SumOfCcjsInLast12Months);
			Map(x => x.SumOfCcjsIn13To24Months);
			Map(x => x.NumOfCcjsInLast24Months);
			Map(x => x.NumOfAssociatedCcjsInLast24Months);
			Map(x => x.SumOfCcjsInLast24Months);
			Map(x => x.SumOfAssociatedCcjsInLast24Months);
			Map(x => x.IsActive);
			HasMany(x => x.HistoryScores).
				KeyColumn("NonLimitedResultId")
				.Cascade.All();
		}
	}

	public interface IExperianNonLimitedResultsRepository : IRepository<ExperianNonLimitedResults>
	{
	}

	public class ExperianNonLimitedResultsRepository : NHibernateRepositoryBase<ExperianNonLimitedResults>, IExperianNonLimitedResultsRepository
	{
		public ExperianNonLimitedResultsRepository(ISession session)
			: base(session)
		{
		}
	}
}