namespace Ezbob.Backend.ModelsWithDB {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;
	using EZBob.DatabaseLib.Model.Database;

	[DataContract]
	public class SignupCustomerMultiOriginModel {
		[DataMember]
		public string UserName {
			get { return (this.userName ?? string.Empty).Trim().ToLowerInvariant(); }
			set { this.userName = (value ?? string.Empty).Trim().ToLowerInvariant(); }
		} // UserName

		[DataMember]
		public CustomerOriginEnum? Origin { get; set; }

		[DataMember]
		public DasKennwort RawPassword { get; set; }

		[DataMember]
		public DasKennwort RawPasswordAgain { get; set; }

		[DataMember]
		public int? PasswordQuestion { get; set; }

		[DataMember]
		public string PasswordAnswer { get; set; }

		[DataMember]
		public string RemoteIp { get; set; }

		[DataMember]
		public string FirstName { get; set; }

		[DataMember]
		public string LastName { get; set; }

		[DataMember]
		public bool CaptchaMode { get; set; }

		[DataMember]
		public string MobilePhone { get; set; }

		[DataMember]
		public string MobileVerificationCode { get; set; }

		[DataMember]
		public bool BrokerFillsForCustomer { get; set; }

		[DataMember]
		public int WhiteLabelID { get; set; }

		/// <summary>
		/// True/false means "set Customer.IsTest field to true/false respectively".
		/// Null means "detect from customer email using TestCustomer table".
		/// </summary>
		[DataMember]
		public bool? IsTest { get; set; }

		[DataMember]
		public CampaignSourceRef CampaignSourceRef { get; set; }

		[DataMember]
		public string GoogleCookie { get; set; }

		[DataMember]
		public string ReferenceSource { get; set; }

		[DataMember]
		public string AlibabaID { get; set; }

		[DataMember]
		public string ABTesting { get; set; }

		[DataMember]
		public string VisitTimes { get; set; }

		[DataMember]
		public string FirstVisitTime { get; set; }

		[DataMember]
		public string RequestedLoanAmount { get; set; }

		[DataMember]
		public string RequestedLoanTerm { get; set; }

		[DataMember]
		public int BrokerLeadID { get; set; }

		[DataMember]
		public string BrokerLeadEmail { get; set; }

		[DataMember]
		public string BrokerLeadFirstName { get; set; }

		public bool IsAlibaba() { return !string.IsNullOrWhiteSpace(AlibabaID); }

		public string FUrl() { return HasSrcRef() ? CampaignSourceRef.FUrl : null; }
		public string FSource() { return HasSrcRef() ? CampaignSourceRef.FSource : null; }
		public string FMedium() { return HasSrcRef() ? CampaignSourceRef.FMedium : null; }
		public string FTerm() { return HasSrcRef() ? CampaignSourceRef.FTerm : null; }
		public string FContent() { return HasSrcRef() ? CampaignSourceRef.FContent : null; }
		public string FName() { return HasSrcRef() ? CampaignSourceRef.FName : null; }
		public DateTime? FDate() { return HasSrcRef() ? CampaignSourceRef.FDate : null; }
		public string RUrl() { return HasSrcRef() ? CampaignSourceRef.RUrl : null; }
		public string RSource() { return HasSrcRef() ? CampaignSourceRef.RSource : null; }
		public string RMedium() { return HasSrcRef() ? CampaignSourceRef.RMedium : null; }
		public string RTerm() { return HasSrcRef() ? CampaignSourceRef.RTerm : null; }
		public string RContent() { return HasSrcRef() ? CampaignSourceRef.RContent : null; }
		public string RName() { return HasSrcRef() ? CampaignSourceRef.RName : null; }
		public DateTime? RDate() { return HasSrcRef() ? CampaignSourceRef.RDate : null; }

		public bool BrokerLeadIsSet() {
			return (BrokerLeadID > 0) && !string.IsNullOrWhiteSpace(BrokerLeadEmail);
		} // BrokerLeadIsSet

		private bool HasSrcRef() { return !BrokerFillsForCustomer && (CampaignSourceRef != null); }

		private string userName;
	} // class SignupCustomerMultiOriginModel
} // namespace
