namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class QuickOfferActionResult : ActionResult {

		public QuickOfferActionResult() {
			HasValue = false;
			Value = null;
		} // constructor

		[DataMember]
		public bool HasValue { get; set; } // HasValue

		[DataMember]
		public QuickOfferModel Value { get; set; } // Value

	} // class QuickOfferActionResult

} // namespace EzService
