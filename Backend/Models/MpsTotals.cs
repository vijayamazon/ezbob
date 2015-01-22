namespace Ezbob.Backend.Models
{
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class MpsTotals
	{
		[DataMember]
		public double TotalSumOfOrders3MTotal { get; set; }

		[DataMember]
		public int MarketplaceSeniorityDays { get; set; }

		[DataMember]
		public decimal TotalSumOfOrdersForLoanOffer { get; set; }

		[DataMember]
		public double TotalSumOfOrders1YTotalForRejection { get; set; }

		[DataMember]
		public double TotalSumOfOrders3MTotalForRejection { get; set; }

		[DataMember]
		public double Yodlee1YForRejection { get; set; }

		[DataMember]
		public double Yodlee3MForRejection { get; set; }
	}
}
