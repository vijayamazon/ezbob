namespace Ezbob.Backend.Models.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	[DataContract]
	public class NL_Model {

		public NL_Model() { }

		public int CustomerID { get; set; }

		[DataMember]
		public NL_FundTransfers FundTransfer { get; set; }

		[DataMember]
		public NL_Loans Loan { get; set; }

		[DataMember]
		public NL_LoanHistory LoanHistory { get; set; }

		// NL_OfferForLoan
		[DataMember]
		public decimal LoanLegalAmount { get; set; }
		[DataMember]
		public int LoanLegalRepaymentPeriod { get; set; }
		[DataMember]
		public string DiscountPlan { get; set; }


		[DataMember]
		public NL_Offers Offer { get; set; }

		[DataMember]
		public NL_LoanLegals LoanLegal { get; set; }

		[DataMember]
		public List<NL_LoanFees> LoanFees { get; set; }

		[DataMember]
		public NL_PacnetTransactions PacnetTransaction { get; set; }




		// lookup objects
		[DataMember]
		public List<NL_PacnetTransactionStatuses> PacnetTransactionStatuses { get; set; }
		[DataMember]
		public List<NL_LoanStatuses> LoanStatuses { get; set; }
		[DataMember]
		public List<NL_LoanFeeTypes> LoanFeeTypes { get; set; }
		[DataMember]
		public List<NL_RepaymentIntervalTypes> RepaymentIntervalTypes { get; set; }
		// ### lookup objects

		

		// schedules
		[DataMember]
		public int ScheduleID { get; set; }
		[DataMember]
		public int SchedulePosition { get; set; }
		[DataMember]
		public DateTime SchedulePayDate { get; set; }
		[DataMember]
		public DateTime ScheduleCloseTime { get; set; }
		[DataMember]
		public decimal SchedulePrincipal { get; set; }
		[DataMember]
		public decimal ScheduleInterestRate { get; set; }
		[DataMember]
		public decimal SchedulePrincipalPaid { get; set; }
		[DataMember]
		public decimal ScheduleInterestPaid { get; set; }


		// payment
		[DataMember]
		public int PaymentID { get; set; }
		[DataMember]
		public int PaymentStatusID { get; set; }
		[DataMember]
		public DateTime PaymentCreateTime { get; set; }
		[DataMember]
		public DateTime PaymentTime { get; set; }
		[DataMember]
		public DateTime PaymentDeletionTime { get; set; }
		[DataMember]
		public int PaymentDeletedByUser { get; set; }
		[DataMember]
		public int PaymentNotes { get; set; }
		[DataMember]
		public int PaymentMethodID { get; set; }


		// paypoint transaction
		[DataMember]
		public decimal PaypointTransactionAmount { get; set; }
		[DataMember]
		public int PaypointTransactionID { get; set; }
		[DataMember]
		public int PaypointTransactionStatusID { get; set; }
		[DataMember]
		public DateTime PaypointTransactionTime { get; set; }

	}



	public enum PacnetTransactionStatus {
		Submited = 1,
		//2	ConfigError:MultipleCandidateChannels
		Error = 3,
		InProgress = 4,
		//PaymentByCustomer = 5,
		Done = 6
	}
}

