using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
	using System;

	#region class ScoringResult

	public class ScoringResult
	{
		public virtual int Id { get; set; }
		public virtual int CustomerId { get; set; }
		public virtual string ACParameters { get; set; }
		public virtual string ACDescription { get; set; }
		public virtual string Weights { get; set; }
		public virtual string MAXPossiblePoints { get; set; }
		public virtual DateTime ScoreDate { get; set; }
		public virtual string Medal { get; set; }
		public virtual double ScorePoints { get; set; }
		public virtual double ScoreResult { get; set; }
	} // class ScoringResult

	#endregion class ScoringResult

    public class ScoringResultMap : ClassMap<ScoringResult>
    {
        public ScoringResultMap()
        {
            Table("CustomerScoringResult");
            Id(x => x.Id).GeneratedBy.Native().Column("Id");
            Map(x => x.ACDescription, "AC_Descriptors");
            Map(x => x.ACParameters, "AC_Parameters");
            Map(x => x.Weights, "Result_Weights");
            Map(x => x.MAXPossiblePoints, "Result_MAXPossiblePoints");
            Map(x => x.CustomerId);
            Map(x => x.ScoreDate);
            Map(x => x.Medal);
            Map(x => x.ScorePoints);
            Map(x => x.ScoreResult);
        }
    }
}


namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using Database;
	using NHibernate;

	public interface IScoringResultRepository : IRepository<ScoringResult>
	{
		IEnumerable<ScoringResult> GetOldMedalsBefore(int customerId, DateTime beforeDate);
		IEnumerable<ScoringResult> GetAllOldMedals(int customerId);
	}

	public class ScoringResultRepository : NHibernateRepositoryBase<ScoringResult>, IScoringResultRepository
	{
		public ScoringResultRepository(ISession session)
			: base(session)
		{
		}

		public IEnumerable<ScoringResult> GetOldMedalsBefore(int customerId, DateTime beforeDate) {
			return GetAll().Where(x => x.CustomerId == customerId && x.ScoreDate < beforeDate);
		}

		public IEnumerable<ScoringResult> GetAllOldMedals(int customerId) {
			return GetAll().Where(x => x.CustomerId == customerId);
		}
	}
}
