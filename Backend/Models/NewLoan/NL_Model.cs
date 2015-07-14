namespace Ezbob.Backend.Models.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using System.Text;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	[DataContract]
	public class NL_Model {

		public NL_Model() {}

		public NL_Model(int customerID) {
			CustomerID = customerID;
			Schedule = new List<NLScheduleItem>();
			Fees = new List<NLFeeItem>();
		}
	
		[DataMember]
		public int CustomerID { get; set; }

		[DataMember]
		public int? UserID { get; set; }

		[DataMember]
		public NL_FundTransfers FundTransfer { get; set; }

		[DataMember]
		public NL_Loans Loan { get; set; }

		// used in agreement builder upon adding new loan
		[DataMember]
		public List<NL_LoanAgreements> LoanAgreements { get; set; }
		// used in LoanCreator.cs on adding new loan
		[DataMember]
		public string AgreementModel { get; set; }
		[DataMember]
		public NL_PacnetTransactions PacnetTransaction { get; set; }
		[DataMember]
		public string PacnetTransactionStatus { get; set; }
		// ### used in LoanCreator.cs on adding new loan


		[DataMember]
		public List<NLScheduleItem> Schedule { get; set; }

		[DataMember]
		public List<NLFeeItem> Fees { get; set; }

		

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


	

		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": \n");
			Type t = typeof(NL_Model);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null) {
					if (prop.Name != "AgreementModel") {
						sb.Append(prop.Name)
							.Append(": " + prop.GetValue(this))
							.Append("; \n");
					}
				}
			}
			return sb.ToString();
		}

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


	}



	//public enum PacnetTransactionStatus {
	//	Submited = 1,
	//	//2	ConfigError:MultipleCandidateChannels
	//	Error = 3,
	//	InProgress = 4,
	//	//PaymentByCustomer = 5,
	//	Done = 6
	//}
}

