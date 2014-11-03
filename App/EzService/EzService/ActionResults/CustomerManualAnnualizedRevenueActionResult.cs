namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class CustomerManualAnnualizedRevenueActionResult : ActionResult {
		[DataMember]
		public CustomerManualAnnualizedRevenue Value { get; set; }
	} // class CustomerManualAnnualizedRevenueActionResult
} // namespace
