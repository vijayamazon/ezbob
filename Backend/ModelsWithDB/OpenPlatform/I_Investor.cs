namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_Investor {
		public I_Investor() {
			InvestorContacts = new List<I_InvestorContact>();
			InvestorBankAccounts = new List<I_InvestorBankAccount>();
			InvestorType = new I_InvestorType();
		}//ctor


        [PK(true)]
        [DataMember]
		public int InvestorID { get; set; }

		[FK("I_InvestorType", "InvestorTypeID")]
        [DataMember]
        public int InvestorTypeID { get; set; }
		
		[Length(255)]
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public decimal? MonthlyFundingCapital { get; set; }

		[DataMember]
		public int? FundsTransferDate { get; set; }

		[DataMember]
		public decimal? DiscountServicingFeePercent { get; set; }

		[DataMember]
		public decimal? FundingLimitForNotification { get; set; }

		[DataMember]
		public bool IsActive { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }
		//////////////////////////////////////////
		
		[DataMember]
		[NonTraversable]
		public I_InvestorType InvestorType { get; set; }

		[DataMember]
		[NonTraversable]
		public List<I_InvestorContact> InvestorContacts { get; set; }
        
        [DataMember]
		[NonTraversable]
		public List<I_InvestorBankAccount> InvestorBankAccounts { get; set; }
        
        [NonTraversable]
        public List<I_InvestorSystemBalance> InvestorSystemBalance { get; set; }

        
	}//class I_Investor
}//ns
