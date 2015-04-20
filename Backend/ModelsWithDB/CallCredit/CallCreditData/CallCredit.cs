namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCredit {
		public CallCredit() {
			ApplicantData = new List<CallCreditData>();
			Amendments = new List<CallCreditAmendments>();
			ApplicantAddresses = new List<CallCreditApplicantAddresses>();
			ApplicantNames = new List<CallCreditApplicantNames>();
			Email = new List<CallCreditEmail>();
			Telephone = new List<CallCreditTelephone>();
		}

		[PK]
		[NonTraversable]
		public long CallCreditID { get; set; }
		[FK("MP_ServiceLog", "ID")]
		public long? MP_ServiceLogId { get; set; }
		public DateTime InsertDate { get; set; }

		public int? CustomerId { get; set; }
		public int? DirectorId { get; set; }
		[Length(4000)]
		public string Error { get; set; }
		public bool HasParsingError { get; set; }
		public bool HasCallCreditError { get; set; }

		public int? LinkType { get; set; }
		[Length(38)]
		public string ReportSearchID { get; set; }

		[Length(4000)]
		public string PayLoadData { get; set; }
		[Length(50)]
		public string YourReference { get; set; }
		[Length(64)]
		public string Token { get; set; }
		[Length(5)]
		public string SchemaVersionCR { get; set; }
		public int? DataSetsCR { get; set; }
		public bool? Score { get; set; }
		[Length(10)]
		public string Purpose { get; set; }
		[Length(10)]
		public string CreditType { get; set; }
		public int? BalorLim { get; set; }
		[Length(10)]
		public string Term { get; set; }
		public bool? Transient { get; set; }
		public bool? AutoSearch { get; set; }
		public int? AutoSearchMaximum { get; set; }
		[Length(38)]
		public string UniqueSearchID { get; set; }
		[Length(1000)]
		public string CastInfo { get; set; }
		public int? PSTV { get; set; }
		public int? LS { get; set; }
		public DateTime? SearchDate { get; set; }
		[Length(5)]
		public string SchemaVersionLR { get; set; }
		public int? DataSetsLR { get; set; }
		[Length(38)]
		public string OrigSrchLRID { get; set; }
		[Length(38)]
		public string NavLinkID { get; set; }
		[Length(5)]
		public string SchemaVersionSR { get; set; }
		public int? DataSetsSR { get; set; }
		[Length(38)]
		public string OrigSrchSRID { get; set; }

		public DateTime? Dob { get; set; }
		public bool? Hho { get; set; }
		public bool? TpOptOut { get; set; }
		[Length(10)]
		public string CustomerStatus { get; set; }
		[Length(10)]
		public string MaritalStatus { get; set; }
		public int? TotalDependents { get; set; }
		[Length(10)]
		public string LanguageVerbal { get; set; }
		[Length(10)]
		public string Type1 { get; set; }
		[Length(10)]
		public string Type2 { get; set; }
		[Length(10)]
		public string Type3 { get; set; }
		[Length(10)]
		public string Type4 { get; set; }
		[Length(10)]
		public string AccommodationType { get; set; }
		public int? PropertyValue { get; set; }
		public int? MortgageBalance { get; set; }
		public int? MonthlyRental { get; set; }
		[Length(10)]
		public string ResidentialStatus { get; set; }
		[Length(10)]
		public string Occupation { get; set; }
		[Length(10)]
		public string EmploymentStatus { get; set; }
		public DateTime? ExpiryDate { get; set; }
		[Length(10)]
		public string EmploymentRecency { get; set; }
		[Length(10)]
		public string EmployerCategory { get; set; }
		[Length(15)]
		public string TimeAtCurrentEmployer { get; set; }
		[Length(6)]
		public string SortCode { get; set; }
		[Length(20)]
		public string AccountNumber { get; set; }
		[Length(15)]
		public string TimeAtBank { get; set; }
		[Length(10)]
		public string PaymentMethod { get; set; }
		[Length(10)]
		public string FinanceType { get; set; }
		public int? TotalDebitCards { get; set; }
		public int? TotalCreditCards { get; set; }
		public int? MonthlyUnsecuredAmount { get; set; }
		public int? AmountPr { get; set; }
		[Length(10)]
		public string TypePr { get; set; }
		[Length(10)]
		public string PaymentMethodPr { get; set; }
		[Length(10)]
		public string FrequencyPr { get; set; }
		public int? AmountAd { get; set; }
		[Length(10)]
		public string TypeAd { get; set; }
		[Length(10)]
		public string PaymentMethodAd { get; set; }
		[Length(10)]
		public string FrequencyAd { get; set; }

		[NonTraversable]
		public List<CallCreditData> ApplicantData { get; set; }
		[NonTraversable]
		public List<CallCreditAmendments> Amendments { get; set; }
		[NonTraversable]
		public List<CallCreditApplicantAddresses> ApplicantAddresses { get; set; }
		[NonTraversable]
		public List<CallCreditApplicantNames> ApplicantNames { get; set; }
		[NonTraversable]
		public List<CallCreditEmail> Email { get; set; }
		[NonTraversable]
		public List<CallCreditTelephone> Telephone { get; set; }
	}
}
