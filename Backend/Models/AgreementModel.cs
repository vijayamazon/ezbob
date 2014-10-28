namespace EzBob.Backend.Models {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[Serializable]
	[DataContract(IsReference = true)]
	public class AgreementModel {
		[DataMember]
		public List<FormattedSchedule> FormattedSchedules { get; set; }
		[DataMember]
		public List<LoanScheduleItemModel> Schedule { get; set; }

		[DataMember]
		public string TotalAmount { get; set; }
		[DataMember]
		public string TotalPrincipal { get; set; }
		[DataMember]
		public string TotalPrincipalUsd { get; set; }

		[DataMember]
		public string TotalInterest { get; set; }
		[DataMember]
		public string TotalFees { get; set; }
		[DataMember]
		public string TotalAmoutOfCredit { get; set; }
		[DataMember]
		public string CompanyAdress { get; set; }
		[DataMember]
		public string CompanyNumber { get; set; }
		[DataMember]
		public string CompanyName { get; set; }

		[DataMember]
		public string TypeOfBusinessName { get; set; }
		[DataMember]
		public string CurentDate { get; set; }
		[DataMember]
		public string PersonAddress { get; set; }

		[DataMember]
		public decimal InterestRate { get; set; }
		[DataMember]
		public double APR { get; set; }
		[DataMember]
		public string SetupFee { get; set; }
		[DataMember]
		public bool IsBrokerFee { get; set; }
		[DataMember]
		public string SetupFeeAmount { get; set; }
		[DataMember]
		public string SetupFeePercent { get; set; }
		[DataMember]
		public bool IsManualSetupFee { get; set; }
		[DataMember]
		public string ManualSetupFee { get; set; }
		[DataMember]
		public int Term;

		[DataMember]
		public string FullName { get; set; }
		[DataMember]
		public string CustomerEmail { get; set; }

		[DataMember]
		public string InterestRatePerDayFormatted { get; set; }
		[DataMember]
		public string InterestRatePerYearFormatted { get; set; }
		[DataMember]
		public decimal InterestRatePerDay { get; set; }

		[DataMember]
		public DateTime CurrentDate { get; set; }

		[DataMember]
		public string LoanType { get; set; }
		[DataMember]
		public int TermOnlyInterest { get; set; }
		[DataMember]
		public string TermOnlyInterestWords { get; set; }
		[DataMember]
		public int TermInterestAndPrincipal { get; set; }
		[DataMember]
		public string TermInterestAndPrincipalWords { get; set; }

		[DataMember]
		public bool isHalwayLoan { get; set; }
		[DataMember]
		public int CountRepayment { get; set; }
		[DataMember]
		public string TotalPrincipalWithSetupFee { get; set; }

		public override string ToString() {
			return "AgreementModel";
		}
	}
}