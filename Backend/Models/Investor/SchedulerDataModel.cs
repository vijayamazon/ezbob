namespace Ezbob.Backend.Models.Investor {
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class SchedulerDataModel {
		
		[DataMember]
		public decimal MonthlyFundingCapital { get; set; }

		[DataMember]
		public string FundsTransferSchedule { get; set; }

		[DataMember]
		public int FundsTransferDate { get; set; }

		[DataMember]
		public string RepaymentsTransferSchedule { get; set; }
	}
}
