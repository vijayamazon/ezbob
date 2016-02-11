namespace EzBob.Backend.Models {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
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

		public string CurrentDateUK {
			get {
				string suffix = "th";

				switch (CurrentDate.Day) {
				case 11:
				case 12:
				case 13:
					// already set
					break;

				default:
					switch (CurrentDate.Day % 10) {
					case 1:
						suffix = "st";
						break;

					case 2:
						suffix = "nd";
						break;

					case 3:
						suffix = "rd";
						break;
					} // switch
					break;
				} // switch

				return CurrentDate.Day + suffix + CurrentDate.ToString(" MMM yyyy", new CultureInfo("en-GB", false));
			} // get
		} // CurrentDateUK

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
        [DataMember]
        public string LatePaymentCharge { get; set; }
        [DataMember]
        public string AdministrationCharge { get; set; }
        [DataMember]
        public bool IsEverlineRefinanceLoan { get; set; }
        [DataMember]
        public string EverlineRefinanceLoanRef { get; set; }
        [DataMember]
        public string EverlineRefinanceLoanDate { get; set; }
        [DataMember]
        public string EverlineRefinanceLoanOutstandingAmount { get; set; }

		public override string ToString() {
			return "AgreementModel";
		}
	}
}