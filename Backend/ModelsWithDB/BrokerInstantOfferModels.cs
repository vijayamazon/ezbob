namespace Ezbob.Backend.ModelsWithDB {
	using System;
	using System.Runtime.Serialization;
	using Utils;
	using Utils.dbutils;

	[DataContract(IsReference = true)]
	public class BrokerInstantOfferRequest {
		
		public BrokerInstantOfferRequest() {
			Created = DateTime.UtcNow;
		}

		[NonTraversable, DataMember, PK]
		public int Id { get; set; }

		[DataMember]
		public DateTime Created { get; set; }

		[DataMember, FK("Broker", "BrokerID")]
		public int BrokerId { get; set; }

		[DataMember]
		public string CompanyNameNumber { get; set; }

		[DataMember]
		public decimal AnnualTurnover { get; set; }

		[DataMember]
		public decimal AnnualProfit { get; set; }

		[DataMember]
		public int NumOfEmployees { get; set; }

		[DataMember]
		public bool IsHomeOwner { get; set; }

		[DataMember]
		public string MainApplicantCreditScore { get; set; }

		[DataMember]
		public string ExperianRefNum { get; set; }

		[DataMember]
		public string ExperianCompanyName { get; set; }

		[DataMember]
		public string ExperianCompanyLegalStatus { get; set; }

		[DataMember]
		public string ExperianCompanyPostcode { get; set; }
	}

	[DataContract(IsReference = true)]
	public class BrokerInstantOfferResponse {

		[NonTraversable, DataMember, PK]
		public int Id { get; set; }

		[DataMember, FK("BrokerInstantOfferRequest", "Id")]
		public int BrokerInstantOfferRequestId { get; set; }
		
		[DataMember]
		public int ApprovedSum { get; set; }

		[DataMember]
		public int RepaymentPeriod { get; set; }

		[DataMember]
		public decimal InterestRate { get; set; }

		[DataMember, FK("LoanType", "Id")]
		public int LoanTypeId { get; set; }

		[DataMember, FK("LoanSource", "LoanSourceID")]
		public int LoanSourceId { get; set; }
		
		[DataMember]
		public bool UseBrokerSetupFee { get; set; }

		[DataMember]
		public bool UseSetupFee { get; set; }
	} 
} 

