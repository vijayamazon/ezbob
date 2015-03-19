namespace Ezbob.Backend.Models {
	using System;
	using System.Collections;
	using System.Runtime.Serialization;
	using Newtonsoft.Json;

	[DataContract]
	public class CustomerRiskData //: IEnumerable
	{
		/*[DataMember]
		public string bankCode { get; set; }
		[DataMember]
		public string bizType { get; set; }
		[DataMember]
		public string requestID { get; set; } // random*/


	/* 0001
	 * bankCode, bizType, requestId, aliMemberId, aId, loanId, requestedAmt, countryId, compName, compStreetAddr1, compStreetAddr2, compCity, compState, compZip, compPhone, compEstablished, compEstablishedYears, compEmployees, compEntityType, compType, firstName, lastName, personalStreet1, personalStreet2, personalCity, personalState, personalZip, personalPhone, personalphoneAlt, email, compRevenue, compNetProfit, compCreLoans, compRent, compOtherLoans, compOtherLeases, personalIncome, compOwnershipPercent, applicationDate, locOfferStatus, locOfferAmount, locOfferCurrency, locOfferDate, locOfferExpireDate */
		[DataMember]
		public string aliMemberId { get; set; } //
		[DataMember]
		public string aId { get; set; } //
		[DataMember]
		public string loanId { get; set; }
		[DataMember]
		public decimal requestedAmt { get; set; }
		[DataMember]
		public string countryId { get; set; } //
		[DataMember]
		public string compName { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string compStreetAddr1 { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string compStreetAddr2 { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string compCity { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string compState { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string compZip { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string compPhone { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public DateTime compEstablished { get; set; }

		/* yyyy-MM-dd */
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string compEstablishedYears { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public int compEmployees { get; set; }
		/* "NONE", "SOLE_PROPRIETOR", "LLC", "PARTNERSHIP", "LLP", "S_CORP", "C_CORP" */
		[DataMember]
		public string compEntityType { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string compType { get; set; }
		[DataMember]
		public string firstName { get; set; }
		[DataMember]
		public string lastName { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string personalStreet1 { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string personalStreet2 { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string personalCity { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string personalState { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string personalZip { get; set; }
		[DataMember]
		public string personalPhone { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string personalPhoneAlt { get; set; }
		[DataMember]
		public string email { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public decimal compRevenue { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public decimal compNetProfit { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public decimal compCreLoans { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public decimal compRent { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public decimal compOtherLoans { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public decimal compOtherLeases { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string compOwnershipPercent { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public decimal personalIncome { get; set; }

		[DataMember] //yyyy-MM-dd hh:mm:ss US PT
		public DateTime applicationDate { get; set; }

/**
* "Offer Selected”
"App Submitted”
"No Loan”*
"Incomplete”*
"More Information Needed”	*/
		[DataMember]
		public string locOfferStatus { get; set; }

		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public decimal locOfferAmount { get; set; }

		[DataMember] // ISO_4217 currencies e.g., USD, GBP
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string locOfferCurrency { get; set; }


		[DataMember] // yyyy-MM-dd hh:mm:ss US PT
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public DateTime locOfferDate { get; set; }

		[DataMember] // yyyy-MM-dd hh:mm:ss US PT
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public DateTime locOfferExpireDate { get; set; }

		/* 002
		 *   compRegId compDba website compName compStreetAddr1 compStreetAddr2 compCity compState compZip compPhone compEstablished compEstablishedYears compEmployees compEntityType compType firstName lastName gender personalStreet1 personalStreet2 personalCity personalState personalZip personalPhone personalPhoneAlt email compRevenue compNetProfit compCreLoans compRent compOtherLoans compOtherLeases personalIncome compOwnershipPercent locApproveStatus locApproveAmount locApproveCurrency locApproveDate locExpireDate remarks rejectReason
		*/

		/* Not provided if company is a Sole Proprietor */
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string compRegId { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string compDba { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string website { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string gender { get; set; } //"M" or "F"
		[DataMember] 
		public string locApproveStatus { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public decimal locApproveAmount { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string locApproveCurrency { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public DateTime locApproveDate { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public DateTime locExpireDate { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string remarks { get; set; }
		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string rejectReason { get; set; }
		

		/*public IEnumerator GetEnumerator() {
			return (IEnumerator)this;
		}*/
	}
}
