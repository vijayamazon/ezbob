namespace Ezbob.Backend.Models.Investor {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class InvestorModel {
		[DataMember]
		public int InvestorID { get; set; }

		[DataMember]
		public InvestorTypeModel InvestorType { get; set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public bool IsActive { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }

		[DataMember]
		public int? FundsTransferDate { get; set; }
		
		[DataMember]
		public decimal? MonthlyFundingCapital { get; set; }
		
		[DataMember]
		public decimal? FundingLimitForNotification { get; set; }

		[DataMember]
		public List<InvestorContactModel> Contacts { get; set; }

		[DataMember]
		public List<InvestorBankAccountModel> Banks { get; set; }
	}

	[DataContract(IsReference = true)]
	public class InvestorTypeModel {
		[DataMember]
		public int InvestorTypeID { get; set; }

		[DataMember]
		public string Name { get; set; }
	}
}
