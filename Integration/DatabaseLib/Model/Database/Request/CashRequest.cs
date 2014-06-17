﻿namespace EZBob.DatabaseLib.Model.Database
{
	using System.Collections.Generic;
	using NHibernate.Type;
	using System;
	using ApplicationMng.Repository;
	using Loans;
	using Model.Loans;
	using NHibernate;

	public class CashRequest
	{
		private int _repaymentPeriod = 3;
		private decimal _interestRate = 0.06M;
		private bool _useSetupFee = true;
		private LoanType _loanType;

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

		public virtual decimal InterestRate
		{
			get { return _interestRate; }
			set { _interestRate = value; }
		}

		public virtual decimal? APR { get; set; }
		public virtual int RepaymentPeriod
		{
			get { return _repaymentPeriod; }
			set { _repaymentPeriod = value; }
		}

		public virtual bool UseSetupFee
		{
			get { return _useSetupFee; }
			set { _useSetupFee = value; }
		}

		public virtual bool UseBrokerSetupFee { get; set; }

		public virtual int? ManualSetupFeeAmount { get; set; }
		public virtual decimal? ManualSetupFeePercent { get; set; }

		public virtual bool IsSure { get; set; }

		public virtual decimal ApprovedSum()
		{
			return (decimal)(ManagerApprovedSum ?? (SystemCalculatedSum ?? 0));
		}

		/// <summary>
		/// Запретить отправку писем пользователю
		/// </summary>
		public virtual bool EmailSendingBanned { get; set; }

		public virtual LoanType LoanType
		{
			get { return _loanType; }
			set { _loanType = value; }
		}

		public virtual bool HasLoans { get; set; }

		public virtual string LoanTemplate { get; set; }

		public virtual int IsLoanTypeSelectionAllowed { get; set; }

		public virtual DiscountPlan DiscountPlan { get; set; }

		public virtual LoanSource LoanSource { get; set; }

		public virtual bool IsCustomerRepaymentPeriodSelectionAllowed { get; set; }

		public virtual IList<LoanLegal> LoanLegals { get; set; }

		public virtual int AnnualTurnover { get; set; }

		public virtual QuickOffer QuickOffer { get; set; }

		public virtual int? ExpirianRating { get; set; }
		public virtual double? ScorePoints { get; set; }

		public virtual CashRequestOriginator? Originator { get; set; }

		public virtual Iesi.Collections.Generic.ISet<DecisionHistory> DecisionHistories { get; set; }
	} // class CashRequest

	public class CashRequestOriginatorType : EnumStringType<CashRequestOriginator> { }
	
	public interface ICashRequestRepository : IRepository<CashRequest>
	{

	}

	public class CashRequestRepository : NHibernateRepositoryBase<CashRequest>, ICashRequestRepository
	{
		public CashRequestRepository(ISession session)
			: base(session)
		{
		}
	}
}
