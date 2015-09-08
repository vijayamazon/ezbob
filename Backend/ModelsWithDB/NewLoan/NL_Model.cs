namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Utils.Attributes;

	[DataContract]
	public class NL_Model : AStringable {
	
		public NL_Model(int customerID) {
			CustomerID = customerID;

			Offer = new NL_Offers();
			Loan = new NL_Loans();
			DiscountPlan = new List<decimal>();
			Agreements = new List<NLAgreementItem>();
			FundTransfer = new NL_FundTransfers();
			//AgreementModel = string.Empty;
			Payment = new NL_Payments();


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
		[ExcludeFromToString]
		public List<NLAgreementItem> Agreements { get; set; }

		//[DataMember]
		//public string AgreementModel { get; set; }

		[DataMember]
		public decimal? BrokerComissions { get; set; }

		[DataMember]
		public List<decimal> DiscountPlan { get; set; }

		[DataMember]
		public NL_Offers Offer { get; set; }

		[DataMember]
		public string Error { get; set; }

		[DataMember]
		public decimal? APR { get; set; }

		[DataMember]
		public string CalculatorImplementation { get; set; }  // AloanCalculator BankLikeLoanCalculator/BankLikeLoanCalculator

	

	


		[DataMember]
		public NL_Payments Payment { get; set; }
		//[DataMember]
		//public NL_PaypointTransactions PaypointTransaction { get; set; }
		//[DataMember]
		//public string PaypointTransactionStatus { get; set; }




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

		// AssignPaymentToLoan strategy
		// 1. argument for the strategy - logic payment to assign (distribute) to loan
		//[DataMember]
		//public NL_Payments PaymentToAssign { get; set; }
		//// 2. result used in AssignPaymentToLoan strategy: loan fees that covered by the amount/or NL_Payments
		//[DataMember]
		//public List<NL_LoanFeePayments> PaymentAssignedToLoanFees { get; set; }
		//// 2. result used in AssignPaymentToLoan strategy: schedule items that covered by the amount/or NL_Payments
		//[DataMember]
		//public List<NL_LoanSchedulePayments> PaymentAssignedToScheduleItems { get; set; }

/*
		protected override bool DisplayFieldInToString(string fieldName) {
			return fieldName != "AgreementModel";
		} // DisplayFieldInToString*/
	} // class NL_Model
} // namespace
