namespace EZBob.DatabaseLib.Model.Database {
	using System.Collections.Generic;
	using NHibernate.Type;
	using System;
	using ApplicationMng.Repository;
	using Loans;
	using Model.Loans;
	using NHibernate;

	public class CashRequest {
		public virtual long Id { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual int? IdUnderwriter { get; set; }
		public virtual DateTime? CreationDate { get; set; }
		public virtual SystemDecision? SystemDecision { get; set; }
		public virtual CreditResultStatus? UnderwriterDecision { get; set; }
		public virtual DateTime? SystemDecisionDate { get; set; }
		public virtual DateTime? UnderwriterDecisionDate { get; set; }
		public virtual DateTime? EscalatedDate { get; set; }
		public virtual double? SystemCalculatedSum { get; set; }
		public virtual double? ManagerApprovedSum { get; set; }
		public virtual Medal? MedalType { get; set; }
		public virtual string EscalationReason { get; set; }
		public virtual string UnderwriterComment { get; set; }

		public virtual DateTime? OfferStart { get; set; }
		public virtual DateTime? OfferValidUntil { get; set; }

		public virtual decimal InterestRate {
			get { return this.interestRate; }
			set { this.interestRate = value; }
		} // InterestRate

		public virtual decimal? APR { get; set; }
		/// <summary>
		/// The value changes as customer changes the slider if allowed
		/// </summary>
		public virtual int RepaymentPeriod {
			get { return this.repaymentPeriod; }
			set { this.repaymentPeriod = value; }
		} // RepaymentPeriod

		/// <summary>
		/// underwriter approved repayment period
		/// </summary>
		public virtual int? ApprovedRepaymentPeriod { get; set; }

		[Obsolete]
		public virtual bool UseSetupFee {
			get { return this.useSetupFee; }
			set { this.useSetupFee = value; }
		} // UseSetupFee

		[Obsolete]
		public virtual bool UseBrokerSetupFee { get; set; }

		[Obsolete]
		public virtual int? ManualSetupFeeAmount { get; set; }

		public virtual decimal? ManualSetupFeePercent { get; set; }
		public virtual decimal? BrokerSetupFeePercent { get; set; }

		public virtual bool IsSure { get; set; }

		public virtual decimal ApprovedSum() {
			return (decimal)(ManagerApprovedSum ?? (SystemCalculatedSum ?? 0));
		} // ApprovedSum

		/// <summary>
		/// Запретить отправку писем пользователю
		/// </summary>
		public virtual bool EmailSendingBanned { get; set; }

		public virtual LoanType LoanType { get; set; }

		public virtual bool HasLoans { get; set; }

		public virtual string LoanTemplate { get; set; }

		public virtual int IsLoanTypeSelectionAllowed { get; set; }

		public virtual DiscountPlan DiscountPlan { get; set; }

		public virtual LoanSource LoanSource { get; set; }

		public virtual bool IsCustomerRepaymentPeriodSelectionAllowed { get; set; }

		public virtual IList<LoanLegal> LoanLegals { get; set; }

		// This property contains customer turnover used to calculate customer medal.
		public virtual int AnnualTurnover { get; set; }

		public virtual QuickOffer QuickOffer { get; set; }

		public virtual int? ExpirianRating { get; set; }

		public virtual double? ScorePoints { get; set; }

		public virtual CashRequestOriginator? Originator { get; set; }

		public virtual Iesi.Collections.Generic.ISet<DecisionHistory> DecisionHistories { get; set; }

		public virtual int? AutoDecisionID { get; set; }

		public virtual bool? SpreadSetupFee { get; set; }

		public virtual int? ProductSubTypeID { get; set; }
		public virtual bool? HasApprovalChance { get; set; }

		public virtual bool UwUpdatedFees { get; set; }

		private int repaymentPeriod = 3;
		private decimal interestRate = 0.06M;
		private bool useSetupFee = true;
	} // class CashRequest

	public static class CashRequestExt {
		public static bool SpreadSetupFee(this CashRequest cr) {
			if (cr == null)
				return false;

			return cr.SpreadSetupFee.HasValue && cr.SpreadSetupFee.Value;
		} // SpreadSetupFee
	} // class CashRequestExt

	public class CashRequestOriginatorType : EnumStringType<CashRequestOriginator> {}

	public interface ICashRequestRepository : IRepository<CashRequest> {}

	public class CashRequestRepository : NHibernateRepositoryBase<CashRequest>, ICashRequestRepository {
		public CashRequestRepository(ISession session) : base(session) {
		} // constructor
	} // class CashRequestRepository
} // namespace
