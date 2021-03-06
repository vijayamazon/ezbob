﻿namespace Ezbob.Backend.Models {
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class BrokerProperties {
		[DataMember]
		public string ErrorMsg { get; set; }

		[DataMember]
		public int BrokerID { get; set; }

		[DataMember]
		public string BrokerName { get; set; }

		[DataMember]
		public string BrokerRegNum { get; set; }

		[DataMember]
		public string ContactName { get; set; }

		[DataMember]
		public string ContactEmail { get; set; }

		[DataMember]
		public string ContactMobile { get; set; }

		[DataMember]
		public string ContactOtherPhone { get; set; }

		[DataMember]
		public string SourceRef { get; set; }

		[DataMember]
		public string BrokerWebSiteUrl { get; set; }

		[DataMember]
		public int SignedTermsID { get; set; }

		[DataMember]
		public int SignedTextID { get; set; }

		[DataMember]
		public int CurrentTermsID { get; set; }

		[DataMember]
		public int CurrentTextID { get; set; }

		[DataMember]
		public string CurrentTerms { get; set; }

		[DataMember]
		public string LotteryPlayerID { get; set; }

		[DataMember]
		public string LotteryCode { get; set; }

		[DataMember]
		public string LicenseNumber { get; set; }

		[DataMember]
		public bool LinkedBank { get; set; }

		[DataMember]
		public decimal ApprovedAmount { get; set; }

		[DataMember]
		public decimal CommissionAmount { get; set; }

		[DataMember]
		public string BankAccount { get; set; }

		[DataMember]
		public string BankSortCode { get; set; }

		[DataMember]
		public string BankName { get; set; }

		[DataMember]
		public int OriginID { get; set; }

		[DataMember]
		public string Origin { get; set; }

		[DataMember]
		public string FrontendSite { get; set; }

		public override string ToString() {
			return string.Format(
@"
	BrokerID: {0}
	BrokerName: {1}
	BrokerRegNum: {2}
	ContactName: {3}
	ContactEmail: {4}
	ContactMobile: {5}
	ContactOtherPhone: {6}
	SourceRef: {7}
	BrokerWebSiteUrl: {8}
	ErrorMsg: {9}
	CurrentTermsID: {10}
	SignedTermsID: {11}
	Current terms text length: {12}
	LinkedBank: {13}
	ApprovedAmount: {14}
	CommissionAmount: {15}
	Origin ID: {16}
	Origin: {17}
	Front-end site: {18}
",
				BrokerID,
				BrokerName,
				BrokerRegNum,
				ContactName,
				ContactEmail,
				ContactMobile,
				ContactOtherPhone,
				SourceRef,
				BrokerWebSiteUrl,
				ErrorMsg,
				CurrentTermsID,
				SignedTermsID,
				CurrentTerms.Length,
				LinkedBank,
				ApprovedAmount,
				CommissionAmount,
				OriginID,
				Origin,
				FrontendSite
			);
		} // ToString
	} // class BrokerProperties
} // namespace Ezbob.Backend.Strategies.Broker
