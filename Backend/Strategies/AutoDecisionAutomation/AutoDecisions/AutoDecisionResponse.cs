﻿namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions {
	using System;
	using DbConstants;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;

	public class AutoDecisionResponse {
		public AutoDecisionResponse() {
			// Currently (July 2015) it is always false. It was true/false in the past and may be such in the future.
			IsLoanTypeSelectionAllowed = false;

			IsAutoBankBasedApproval = false;

			Decision = null;

			AutoRejectReason = null;
			CreditResult = null;
			UserStatus = null;
			SystemDecision = null;
			LoanOfferUnderwriterComment = null;

			AutoApproveAmount = 0;
			RepaymentPeriod = 0;
			BankBasedAutoApproveAmount = 0;
			DecisionName = "Manual";

			AppValidFor = null;
			LoanOfferEmailSendingBannedNew = false;
			LoanSourceID = 0;
			LoanTypeID = 0;
			InterestRate = 0;
			SetupFee = 0;
			BrokerSetupFeePercent = null;
			SpreadSetupFee = false;
			IsCustomerRepaymentPeriodSelectionAllowed = false;
			DiscountPlanID = null;
			HasApprovalChance = false;
		} // constructor

		public bool IsLoanTypeSelectionAllowed { get; set; }

		public bool IsAutoBankBasedApproval { get; set; }

		public DecisionActions? Decision { get; set; }

		public int? DecisionCode { get { return Decision == null ? (int?)null : (int)Decision; } }

		public bool IsAutoApproval { get { return Decision == DecisionActions.Approve; } }
		public bool IsAutoReApproval { get { return Decision == DecisionActions.ReApprove; } }
		public bool IsReRejected { get { return Decision == DecisionActions.ReReject; } }
		public bool IsRejected { get { return Decision == DecisionActions.Reject; } }

		public bool DecidedToReject { get { return IsReRejected || IsRejected; } }
		public bool DecidedToApprove { get { return IsAutoApproval || IsAutoBankBasedApproval || IsAutoReApproval; } }
		public bool HasAutoDecided { get { return DecidedToApprove || DecidedToReject; } }

		public string AutoRejectReason { get; set; }
		public CreditResultStatus? CreditResult { get; set; } // Rejected / Approved / WaitingForDecision
		public Status? UserStatus { get; set; } // Approved / Manual / Rejected
		public SystemDecision? SystemDecision { get; set; } // Approve / Manual / Reject
		public string LoanOfferUnderwriterComment { get; set; }

		public int AutoApproveAmount { get; set; }
		public int RepaymentPeriod { get; set; }
		public int BankBasedAutoApproveAmount { get; set; }
		public string DecisionName { get; set; } // Manual/Approval/Re-Approval/Bank Based Approval/Rejection/Re-Rejection

		public DateTime? AppValidFor { get; set; }
		public bool LoanOfferEmailSendingBannedNew { get; set; }

		/// <summary>
		/// Loan source ID as decided by auto decision.
		/// </summary>
		/// <remarks>Reader is for internal use only. Externally use LoanSource which is based on this.</remarks>
		public int LoanSourceID { private get; set; }

		/// <summary>
		/// Based on LoanSourceID. Checks that requested loan source id really exists and replaces with default if needed.
		/// </summary>
		public LoanSource LoanSource {
			get {
				if (this.loanSource == null)
					this.loanSource = GetLoanSource();

				return this.loanSource;
			} // get
		} // LoanSource

		/// <summary>
		/// Loan type ID as decided by auto decision.
		/// </summary>
		/// <remarks>Reader is for internal use only. Externally use LoanTypeIDToUse which is based on this.</remarks>
		public int LoanTypeID {
			get {
				if (this.loanTypeIDToUse == null)
					this.loanTypeIDToUse = GetLoanType();

				return this.loanTypeIDToUse.Value;
			} // get
			set {
				this.loanTypeID = value;
				this.loanTypeIDToUse = null;
			} // set
		} // LoanTypeID

		public decimal InterestRate { get; set; }
		public decimal SetupFee { get; set; }
		public decimal? BrokerSetupFeePercent { get; set; }
		public bool SpreadSetupFee { get; set; }
		public bool IsCustomerRepaymentPeriodSelectionAllowed { get; set; }

		/// <summary>
		/// Discount plan as decided by auto decision.
		/// </summary>
		/// <remarks>Reader is for internal use only. Externally use DiscountPlanIDToUse which is based on this.</remarks>
		public int? DiscountPlanID {
			private get { return this.discountPlanID; }
			set {
				this.discountPlanID = value;
				this.discountPlanIDToUse = null;
			}
		} // DiscountPlanID

		/// <summary>
		/// Based on DiscountPlanID. Checks that requested discount plan id really exists
		/// and replaces with default if needed.
		/// </summary>
		public int DiscountPlanIDToUse {
			get {
				if (this.discountPlanIDToUse == null)
					this.discountPlanIDToUse = GetDiscountPlan();

				return this.discountPlanIDToUse.Value;
			} // get
		} // DiscountPlanIDToUse

		public bool HasApprovalChance { get; set; }

		private int GetLoanType() {
			SafeReader ltsr = Library.Instance.DB.GetFirst(
				"GetLoanTypeAndDefault",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LoanTypeID", this.loanTypeID)
			);

			int? dbLoanTypeID = ltsr["LoanTypeID"];
			int defaultLoanTypeID = ltsr["DefaultLoanTypeID"];

			return DecidedToApprove
				? (dbLoanTypeID ?? defaultLoanTypeID)
				: defaultLoanTypeID;
		} // GetLoanType

		private int GetDiscountPlan() {
			SafeReader dpsr = Library.Instance.DB.GetFirst(
				"GetDiscountPlan",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@DiscountPlanID", DiscountPlanID)
			);

			return dpsr["DiscountPlanID"];
		} // GetDiscountPlan

		private LoanSource GetLoanSource() {
			SafeReader lssr = Library.Instance.DB.GetFirst(
				"GetLoanSource",
				CommandSpecies.StoredProcedure,
				new QueryParameter( // Get specific for re-approval or default otherwise
					"@LoanSourceID",
					IsAutoReApproval ? LoanSourceID : (int?)null
				)
			);

			return lssr.Fill<LoanSource>();
		} // GetLoanSource

		private int? loanTypeIDToUse;
		private int loanTypeID;

		private int? discountPlanIDToUse;
		private int? discountPlanID;

		private LoanSource loanSource;
	} // class AutoDecisionResponse
} // namespace
