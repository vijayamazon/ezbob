namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.Serialization;
	using ConfigManager;
	using Ezbob.Utils.Attributes;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;

	[DataContract]
	public class NL_Model : AStringable {
	
		public NL_Model(int customerID) {

			CustomerID = customerID;

			Offer = new NL_Offers();
			Loan = new NL_Loans();
			Agreements = new List<NLAgreementItem>();
			FundTransfer = new NL_FundTransfers();
			CalculatorImplementation = CurrentValues.Instance.DefaultLoanCalculator.Value;

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

		[DataMember]
		public decimal? BrokerComissions { get; set; }

		[DataMember]
		public NL_Offers Offer { get; set; }

		[DataMember]
		public string Error { get; set; }

		[DataMember]
		public decimal? APR { get; set; }
		
		[DataMember]
		public string CalculatorImplementation { get; set; } // AloanCalculator LegacyLoanCalculator/BankLikeLoanCalculator

		/// <exception cref="Exception">Condition. </exception>
		public ALoanCalculator GetCalculatorInstance() {
			try {
				Type myType = Type.GetType(CurrentValues.Instance.DefaultLoanCalculator.Value);
				
				if (myType != null) {

					//default type from configurations
					ALoanCalculator calc = (ALoanCalculator)Activator.CreateInstance(myType, this);

					if (CalculatorImplementation.GetType() == typeof(BankLikeLoanCalculator))
						calc = new BankLikeLoanCalculator(this);
					else if (CalculatorImplementation.GetType() == typeof(LegacyLoanCalculator))
						calc = new LegacyLoanCalculator(this);

					Console.WriteLine(calc);
					
					return calc;
				}
				// ReSharper disable once CatchAllClause
			} catch (Exception e) {
				Console.WriteLine(e);
				// ReSharper disable once ThrowingSystemException
				throw new Exception(string.Format("Failed to create calculator instance for {0}", CalculatorImplementation), e);
			}
			return null;
		}

		//[DataMember]
		//public NL_Payments Payment { get; set; }
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
