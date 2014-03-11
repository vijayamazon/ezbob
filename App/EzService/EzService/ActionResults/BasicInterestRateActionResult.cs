/*TODO - remove if serialized is ok
namespace EzService.ActionResults
{
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	
	public class SingleBasicInterestRate
	{
		[DataMember]
		public int Id { get; set; }
		[DataMember]
		public int FromScore { get; set; }
		[DataMember]
		public int ToScore { get; set; }
		[DataMember]
		public int LoanInterestBase { get; set; }
	}

	#region class BasicInterestRateActionResult

	[DataContract]
	public class BasicInterestRateActionResult : ActionResult
	{
		[DataMember]
		public List<SingleBasicInterestRate> BasicInterestRates { get; set; }
	} // class BasicInterestRateActionResult

	#endregion class BasicInterestRateActionResult
} // namespace EzService*/
