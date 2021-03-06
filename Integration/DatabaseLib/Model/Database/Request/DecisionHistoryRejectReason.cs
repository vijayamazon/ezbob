﻿using ApplicationMng.Repository;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database
{

	public class DecisionHistoryRejectReason
	{
		public virtual int Id { get; set; }
		public virtual DecisionHistory DecisionHistory { get; set; }
		public virtual RejectReason RejectReason { get; set; }
	}

	public class DecisionHistoryRejectReasonMap : ClassMap<DecisionHistoryRejectReason>
	{
		public DecisionHistoryRejectReasonMap()
		{
			Id(x => x.Id);
			References(x => x.DecisionHistory, "DecisionHistoryId");
			References(x => x.RejectReason, "RejectReasonId");
		}
	}

	public interface IDecisionHistoryRejectReasonRepository : IRepository<DecisionHistoryRejectReason>
	{

	}

	public class DecisionHistoryRejectReasonRepository : NHibernateRepositoryBase<DecisionHistoryRejectReason>, IDecisionHistoryRejectReasonRepository
	{
		public DecisionHistoryRejectReasonRepository(ISession session)
			: base(session)
		{
		}
	}
}