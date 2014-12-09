namespace EzService {
	using System.Runtime.Serialization;

	[DataContract]
	public class DecimalActionResult : ActionResult {

		public DecimalActionResult() {
			HasValue = false;
			Value = 0;
		} // constructor

		[DataMember]
		public bool HasValue { get; set; } // HasValue

		[DataMember]
		public decimal Value { get; set; } // Value

	} // class DecimalActionResult

} // namespace EzService
