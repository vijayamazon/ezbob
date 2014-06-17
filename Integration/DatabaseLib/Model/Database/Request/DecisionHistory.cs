﻿namespace EZBob.DatabaseLib.Model.Database
{
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using Iesi.Collections.Generic;
	using Model.Loans;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;
	using System.Collections.Generic;
	using UserManagement;

	public enum DecisionActions
	{
		Approve,
		Reject,
		Escalate,
		Pending,
		Waiting
	}

	public class CreditResultDecisionActionsType : EnumStringType<DecisionActions>
	{
	}

	public class DecisionHistory
	{
		public virtual int Id { get; set; }
		public virtual DecisionActions Action { get; set; }
		public virtual DateTime Date { get; set; }
		public virtual string Comment { get; set; }
		public virtual User Underwriter { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual CashRequest CashRequest { get; set; }
		public virtual LoanType LoanType { get; set; }
		public virtual Iesi.Collections.Generic.ISet<DecisionHistoryRejectReason> RejectReasons { get; set; }
	}

	public interface IDecisionHistoryRepository : IRepository<DecisionHistory>
	{
		void LogAction(DecisionActions action, string comment, User underwriter, Customer customer, IEnumerable<int> rejectionReasons = null);
		IList<DecisionHistory> ByCustomer(Customer customer);
	}

	public class DecisionHistoryRepository : NHibernateRepositoryBase<DecisionHistory>, IDecisionHistoryRepository
	{
		public DecisionHistoryRepository(ISession session)
			: base(session)
		{
		}

		public void LogAction(DecisionActions action, string comment, User underwriter, Customer customer, IEnumerable<int> rejectionReasons = null)
		{
			customer.SystemCalculatedSum = 0;
			customer.ManagerApprovedSum = 0;

			var lastAction = customer.DecisionHistory.OrderBy(d => d.Date).LastOrDefault();

			customer.LastStatus = lastAction == null ? "N/A" : lastAction.Action.ToString();

			if (action == DecisionActions.Approve)
			{
				customer.NumApproves++;

				if (customer.LastCashRequest.SystemCalculatedSum.HasValue)
				{
					customer.SystemCalculatedSum = (decimal)customer.LastCashRequest.SystemCalculatedSum;
				}

				if (customer.LastCashRequest.ManagerApprovedSum.HasValue)
				{
					customer.ManagerApprovedSum = (decimal)customer.LastCashRequest.ManagerApprovedSum;
				}
			}
			else if (action == DecisionActions.Reject)
			{
				customer.NumRejects++;
			}

			var cr = customer.LastCashRequest;
			var item = new DecisionHistory
						   {
							   Date = DateTime.UtcNow,
							   Action = action,
							   Underwriter = underwriter,
							   Customer = customer,
							   Comment = comment,
							   CashRequest = cr,
							   LoanType = cr.LoanType,
							   RejectReasons = new HashedSet<DecisionHistoryRejectReason>()
						   };
			if (rejectionReasons != null)
			{
				foreach (var rejectionReason in rejectionReasons)
				{
					item.RejectReasons.Add(new DecisionHistoryRejectReason
						{
							DecisionHistory = item,
							RejectReasonId = rejectionReason
						});
				}
			}
			Save(item);
		}

		public IList<DecisionHistory> ByCustomer(Customer customer)
		{
			return GetAll().Where(d => d.Customer.Id == customer.Id).ToList();
		}
	}

	public class DecisionHistoryMap : ClassMap<DecisionHistory>
	{
		public DecisionHistoryMap()
		{
			Id(x => x.Id).GeneratedBy.HiLo("100");
			Map(x => x.Date).CustomType<UtcDateTimeType>();
			Map(x => x.Comment).Length(2000);
			References(x => x.Underwriter, "UnderwriterId");
			References(x => x.Customer, "CustomerId");
			References(x => x.CashRequest, "CashRequestId");
			References(x => x.LoanType, "LoanTypeId");
			Map(x => x.Action).CustomType<CreditResultDecisionActionsType>();
			HasMany(x => x.RejectReasons)
				.KeyColumn("DecisionHistoryId")
				.Cascade.All();
		}
	}
}