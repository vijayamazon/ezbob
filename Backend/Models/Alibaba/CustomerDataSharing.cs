namespace Ezbob.Backend.Models.Alibaba {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.Extensions;

	public enum AlibabaLocOfferStatus {
		[Description("Offer Selected")]
		OfferSelected,
		[Description("App Submitted")]
		AppSubmitted,
		[Description("No Loan")]
		NoLoan,
		[Description("Incomplete")]
		Incomplete,
		[Description("More Information Needed")]
		MoreInformationNeeded,
		[Description("Approved")]
		Approved
	}

	/*
790	NULL
7087	Approved
140	ApprovedPending
1	Escalated
10773	Rejected
45	WaitingForDecision
*/

	/*
	 * (No column name)	TypeOfBusiness
364		PShip3P
44		PShip
3888	NULL
5664	Limited
8753	Entrepreneur
1075	SoleTrader
78		LLP
	 * */


	[DataContract]
	public class CustomerDataSharing {
		private string _locOfferStatus;
		private string _locApproveStatus;
		private string _compEntityType;

		[DataMember(EmitDefaultValue = true, IsRequired = true)] // customerID
		public int aId { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true)] //  AlibabaBuyer table
		public long aliMemberId { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true)] // Partner loan ID | Y
		public int? loanId = 0; //{ get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true)] //Requested loan amount
		public decimal? requestedAmt { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true)] // ???
		public string countryId = "UK";

		[DataMember(EmitDefaultValue = true, IsRequired = true)]  // co.ExperianCompanyName
		public string compName { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public string compStreetAddr1 { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public string compStreetAddr2 { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public string compCity { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public string compState { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public string compZip { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public string compPhone { get; set; }

		[DataMember(EmitDefaultValue = true)] // yyyy-MM-dd
		public DateTime? compEstablished { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public string compEstablishedYears { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public int? compEmployees { get; set; }

		
/*
LLP = "LLP"
P Ship3P = "PARTNERSHIP_3P"
P Ship = "PARTNERSHIP"
Limited = "LIMITED"
*/
		[DataMember(EmitDefaultValue = true, IsRequired = true)]
		public string compEntityType {
			get { return this._compEntityType; }
			set {
				switch (value) {
				case "PShip":
					this._compEntityType = "PARTNERSHIP";
					break;
				case "PShip3P":
					this._compEntityType = "PARTNERSHIP_3P";
					break;
				case "LLP":
				case "Limited":
					this._compEntityType = "LIMITED";
					break;
				}
			}
		}

		[DataMember(EmitDefaultValue = true)] // Industry
		public string compType { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true)] // c.FirstName
		public string firstName { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true)] // c.Fullname
		public string lastName { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public string personalStreet1 { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public string personalStreet2 { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public string personalCity { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public string personalState { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public string personalZip { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true)]
		public string personalPhone { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public string personalphoneAlt { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true)]
		public string email { get; set; }

		[DataMember(EmitDefaultValue = false)] // Business revenue last year
		public decimal? compRevenue { get; set; }

		[DataMember(EmitDefaultValue = false)] // Business net profit before taxes last year
		public decimal? compNetProfit { get; set; }

		[DataMember(EmitDefaultValue = false)] // not supported
		public decimal? compCreLoans { get; set; }

		[DataMember(EmitDefaultValue = false)] // not supported
		public decimal? compRent { get; set; }

		[DataMember(EmitDefaultValue = false)] // not supported
		public decimal? compOtherLoans { get; set; }

		[DataMember(EmitDefaultValue = false)] // not supported
		public decimal? compOtherLeases { get; set; }

		[DataMember(EmitDefaultValue = false)] // Other personal income before taxes -- not supported
		public decimal? personalIncome { get; set; }

		[DataMember(EmitDefaultValue = false)] // % ownership of business
		public decimal? compOwnershipPercent { get; set; }

		[DataMember(EmitDefaultValue = true)] // yyyy-MM-dd hh:mm:ssUS PT
		public DateTime applicationDate { get; set; }


		/*
		 * 
		"Offer Selected"=  start wizard 
		"App Submitted"=  WaitingForDecision  
		"No Loan"*=  rejected 
		"Incomplete"*= wizard not complete 
		"More Information Needed"=  pending 
		"Approved"=  approved 
		 */
		// Bank Based Approval ????

		// SystemDecision
		[DataMember(EmitDefaultValue = true, IsRequired = true)] // Front end decision
		public string locOfferStatus {
			get { return this._locOfferStatus; }
			set {
				switch (value) {
				case "Escalated":
					this._locOfferStatus = AlibabaLocOfferStatus.MoreInformationNeeded.DescriptionAttr();
					break;

				case "ApprovedPending":
				case "WaitingForDecision":
				case "Manual":
					this._locOfferStatus = AlibabaLocOfferStatus.Incomplete.DescriptionAttr();
					break;

				case "Approval":
				case "Re-Approval":
				case "Approved":
					this._locOfferStatus = AlibabaLocOfferStatus.Approved.DescriptionAttr();
					break;

				case "Rejected":
				case "Re-Rejection":
				case "Rejection":
					this._locOfferStatus = AlibabaLocOfferStatus.NoLoan.DescriptionAttr();
					break;
				default:
					this._locOfferStatus = AlibabaLocOfferStatus.Incomplete.DescriptionAttr();
					break;
				}
			}
		}

		[DataMember(EmitDefaultValue = false)] //Offered amount; Set to "0" if offer not available at this point
		public decimal locOfferAmount = 0;

		[DataMember(EmitDefaultValue = true)] // not from SP
		public string locOfferCurrency = "GBP";

		[DataMember(EmitDefaultValue = false)] // Date LOC was approved; yyyy-MM-dd hh:mm:ssUS PT
		public DateTime? locOfferDate { get; set; }

		[DataMember(EmitDefaultValue = false)] //Expiration date for the Line of Credit; yyyy-MM-dd hh:mm:ssUS PT | N
		public DateTime? locOfferExpireDate { get; set; }

		[DataMember(EmitDefaultValue = false)] // Business tax ID, US: EIN/Tax ID UK: CRN	|Not provided if company is a Sole Proprietor |	N
		public string compRegId { get; set; }

		[DataMember(EmitDefaultValue = false)] // DBA (doing business as) |N
		public string compDba { get; set; }

		[DataMember(EmitDefaultValue = false)] // Company website URL | N
		public string website { get; set; }

		[DataMember(EmitDefaultValue = false)]  // "M" or "F" | N
		public string gender { get; set; }

		// UnderwriteDecision
		[DataMember(EmitDefaultValue = true, IsRequired = true)] // Final LOC approval status: "Approved" "No Loan" "App Submitted" | Y
		public string locApproveStatus {
			get { return this._locApproveStatus; }
			set {
				switch (value) {
				case "Approval":
				case "Re-Approval":
				case "Approved":
					this._locApproveStatus = AlibabaLocOfferStatus.Approved.DescriptionAttr();
					break;

				case "Rejected":
				case "Re-Rejection":
				case "Rejection":
					this._locApproveStatus = AlibabaLocOfferStatus.NoLoan.DescriptionAttr();
					break;

				default:
					this._locApproveStatus = AlibabaLocOfferStatus.AppSubmitted.DescriptionAttr();
					break;
				}
			}
		}


		[DataMember(EmitDefaultValue = false)] // Approved amount | N
		public decimal? locApproveAmount { get; set; }

		[DataMember(EmitDefaultValue = false)]		// N
		public string locApproveCurrency = "GBP";

		[DataMember(EmitDefaultValue = false)] // Approval date; yyyy-MM-dd hh:mm:ssUS PT |N
		public DateTime? locApproveDate { get; set; }

		[DataMember(EmitDefaultValue = false)] // Credit line valid until; yyyy-MM-dd hh:mm:ssUS PT |N
		public DateTime? locExpireDate { get; set; }

		[DataMember(EmitDefaultValue = false)] // Remarks about application |N
		public string remarks { get; set; }

		[DataMember(EmitDefaultValue = false)] // Reason application declined |N
		public string rejectReason { get; set; }

	}

	public static class CustomerDataSharingExt {
		public static string Stringify(this CustomerDataSharing cds) {
			if (cds == null)
				return "-- null --";

			var os = new List<string>();

			cds.Traverse((instance, propInfo) => {
				os.Add(string.Format("{0} = '{1}'", propInfo.Name, propInfo.GetValue(instance)));
			});

			return string.Format("{{ {0} }}", string.Join(", ", os));
		} // Stringify
	}
}
