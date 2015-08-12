namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	[DataContract]
	public class NL_Model : AStringable {
		public NL_Model() { } // constructor

		public NL_Model(int customerID) {
			CustomerID = customerID;

			Schedule = new List<NLScheduleItem>();
			Fees = new List<NLFeeItem>();
			Agreements = new List<NLAgreementItem>();
		} // constructor

		[DataMember]
		public int CustomerID { get; set; }

		[DataMember]
		public int? UserID { get; set; }

		[DataMember]
		public NL_FundTransfers FundTransfer { get; set; }

		[DataMember]
		public NL_Loans Loan { get; set; }

		[DataMember]
		public decimal InitialAmount { get; set; }

		[DataMember]
		public int InitialRepaymentCount { get; set; }

		[DataMember]
		public int InitialRepaymentIntervalTypeID { get; set; }

		[DataMember]
		public decimal InitialInterestRate { get; set; }

		[DataMember]
		public DateTime IssuedTime { get; set; }

		[DataMember]
		public decimal[] DiscountPlan { get; set; }

		[DataMember]
		public List<NLAgreementItem> Agreements { get; set; }

		[DataMember]
		public string AgreementModel { get; set; }

		[DataMember]
		public decimal? BrokerComissions { get; set; }

		[DataMember]
		public NL_PacnetTransactions PacnetTransaction { get; set; }
		[DataMember]
		public string PacnetTransactionStatus { get; set; }

		[DataMember]
		public List<NLScheduleItem> Schedule { get; set; }
		[DataMember]
		public List<NLFeeItem> Fees { get; set; }

		[DataMember]
		public NL_Offers Offer { get; set; }

		// AssignPaymentToLoan strategy
		// 1. argument for the strategy - logic payment to assign (distribute) to loan
		[DataMember]
		public NL_Payments PaymentToAssign { get; set; }
		// 2. result used in AssignPaymentToLoan strategy: loan fees that covered by the amount/or NL_Payments
		[DataMember]
		public List<NL_LoanFeePayments> PaymentAssignedToLoanFees { get; set; }
		// 2. result used in AssignPaymentToLoan strategy: schedule items that covered by the amount/or NL_Payments
		[DataMember]
		public List<NL_LoanSchedulePayments> PaymentAssignedToScheduleItems { get; set; }

		// "Pay loan" 
		//[DataMember]
		//public string PaypointCardNo { get; set; }
		//[DataMember]
		//public string PaymentStatus { get; set; }
		[DataMember]
		public NL_Payments Payment { get; set; }
		[DataMember]
		public NL_PaypointTransactions PaypointTransaction { get; set; }
		[DataMember]
		public string PaypointTransactionStatus { get; set; }

		[DataMember]
		public string Error { get; set; }

		[DataMember]
		public decimal? APR { get; set; }

		[DataMember]
		public string CalculatorImplementation { get; set; }  // AloanCalculator BankLikeLoanCalculator/BankLikeLoanCalculator

		//[DataMember]
		//public NL_LoanHistory LoanHistory { get; set; }

		// lookup objects
		//[DataMember]
		//public List<NL_PacnetTransactionStatuses> PacnetTransactionStatuses { get; set; }
		//[DataMember]
		//public List<NL_LoanStatuses> LoanStatuses { get; set; }
		//[DataMember]
		//public List<NL_LoanFeeTypes> LoanFeeTypes { get; set; }
		//[DataMember]
		//public List<NL_RepaymentIntervalTypes> RepaymentIntervalTypes { get; set; }
		// ### lookup objects

		//[DataMember]
		//public NL_Offers Offer { get; set; }
		//[DataMember]
		//public List<NL_LoanSchedules> Schedule { get; set; }
		//[DataMember]
		//public NL_LoanLegals LoanLegal { get; set; }
		//[DataMember]
		//public List<NL_LoanFees> LoanFees { get; set; }

		// payment
		//[DataMember]
		//public int PaymentID { get; set; }
		//[DataMember]
		//public int PaymentStatusID { get; set; }
		//[DataMember]
		//public DateTime PaymentCreateTime { get; set; }
		//[DataMember]
		//public DateTime PaymentTime { get; set; }
		//[DataMember]
		//public DateTime PaymentDeletionTime { get; set; }
		//[DataMember]
		//public int PaymentDeletedByUser { get; set; }
		//[DataMember]
		//public int PaymentNotes { get; set; }
		//[DataMember]
		//public int PaymentMethodID { get; set; }

		// paypoint transaction
		//[DataMember]
		//public decimal PaypointTransactionAmount { get; set; }
		//[DataMember]
		//public int PaypointTransactionID { get; set; }
		//[DataMember]
		//public int PaypointTransactionStatusID { get; set; }
		//[DataMember]
		//public DateTime PaypointTransactionTime { get; set; }

		protected override bool DisplayFieldInToString(string fieldName) {
			return fieldName != "AgreementModel";
		} // DisplayFieldInToString
	} // class NL_Model
} // namespace
