namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using DbConstants;
	using Ezbob.Logger;
	using Iesi.Collections.Generic;
	using Model.Loans;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;
	using System.Collections.Generic;
	using UserManagement;

	public class CreditResultDecisionActionsType : EnumStringType<DecisionActions> {
	} // class CreditResultDecisionActionsType

	public class DecisionHistory {
		public virtual int Id { get; set; }
		public virtual DecisionActions Action { get; set; }
		public virtual DateTime Date { get; set; }
		public virtual string Comment { get; set; }
		public virtual User Underwriter { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual CashRequest CashRequest { get; set; }
		public virtual LoanType LoanType { get; set; }
		public virtual Iesi.Collections.Generic.ISet<DecisionHistoryRejectReason> RejectReasons { get; set; }
	} // class DecisionHistory

	public interface IDecisionHistoryRepository : IRepository<DecisionHistory> {
		void LogAction(
			DecisionActions action,
			string comment,
			User underwriter,
			Customer customer,
			IEnumerable<int> rejectionReasons = null
		);
		IList<DecisionHistory> ByCustomer(Customer customer);
	} // interface IDecisionHistoryRepository

	public class DecisionHistoryRepository : NHibernateRepositoryBase<DecisionHistory>, IDecisionHistoryRepository {
		public DecisionHistoryRepository(ISession session) : base(session) {
		} // constructor

		public void LogAction(
			DecisionActions action,
			string comment,
			User underwriter,
			Customer customer,
			IEnumerable<int> rejectionReasons = null
		) {
			try {
				if ((customer == null) || (customer.LastCashRequest == null))
					return;

				var lastAction = customer.DecisionHistory.OrderBy(d => d.Date).LastOrDefault();

				customer.LastStatus = lastAction == null ? "N/A" : lastAction.Action.ToString();

				var cr = customer.LastCashRequest;

				var item = new DecisionHistory {
					Date = DateTime.UtcNow,
					Action = action,
					Underwriter = underwriter,
					Customer = customer,
					Comment = comment,
					CashRequest = cr,
					LoanType = cr.LoanType,
					RejectReasons = new HashedSet<DecisionHistoryRejectReason>(),
				};

				if (rejectionReasons != null) {
					var repo = new RejectReasonRepository(Session);

					foreach (var rejectionReason in rejectionReasons) {
						var reason = repo.Get(rejectionReason);
						item.RejectReasons.Add(new DecisionHistoryRejectReason {
							DecisionHistory = item,
							RejectReason = reason
						});
					} // for each
				} // if

				if ((action == DecisionActions.Reject) || (action == DecisionActions.ReReject)) {
					string reasons = item.RejectReasons.Any()
						? item.RejectReasons.Select(x => x.RejectReason.Reason).Aggregate((a, b) => a + ", " + b)
						: null;

					customer.RejectedReason = string.IsNullOrEmpty(reasons)
						? comment
						: string.Format("{0} ({1})", reasons, comment);
				} // if

				Save(item);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to log Decision History action.");
			} // try
		} // LogAction

		public IList<DecisionHistory> ByCustomer(Customer customer) {
			return GetAll().Where(d => d.Customer.Id == customer.Id).ToList();
		} // ByCustomer

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(DecisionHistoryRepository));
	} // class DecisionHistoryRepository

	public class DecisionHistoryMap : ClassMap<DecisionHistory> {
		public DecisionHistoryMap() {
			Id(x => x.Id).GeneratedBy.Native();

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
		} // constructor
	} // class DecisionHistoryMap
} // namespace