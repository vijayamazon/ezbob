namespace EzService {
	using System.Runtime.Serialization;

	#region class DecimalActionResult

	[DataContract]
	public class DecimalActionResult : ActionResult {
		#region constructor

		public DecimalActionResult() {
			HasValue = false;
			Value = 0;
		} // constructor

		#endregion constructor

		#region property HasValue

		[DataMember]
		public bool HasValue { get; set; } // HasValue

		#endregion property HasValue

		#region property Value

		[DataMember]
		public decimal Value { get; set; } // Value

		#endregion property Value
	} // class DecimalActionResult

	#endregion class DecimalActionResult
} // namespace EzService
