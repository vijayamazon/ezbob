namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class LotteryActionResult : ActionResult {
		[DataMember]
		public LotteryResult Value { get; set; }
	} // class LotteryActionResult
} // namespace EzService
