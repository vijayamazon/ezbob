namespace EzService {
	using System.Runtime.Serialization;
	using EzBob.Backend.Models;

	#region class QuickOfferActionResult

	[DataContract]
	public class QuickOfferActionResult : ActionResult {
		#region constructor

		public QuickOfferActionResult() {
			HasValue = false;
			Value = null;
		} // constructor

		#endregion constructor

		#region property HasValue

		[DataMember]
		public bool HasValue { get; set; } // HasValue

		#endregion property HasValue

		#region property Value

		[DataMember]
		public QuickOfferModel Value { get; set; } // Value

		#endregion property Value
	} // class QuickOfferActionResult

	#endregion class QuickOfferActionResult
} // namespace EzService
