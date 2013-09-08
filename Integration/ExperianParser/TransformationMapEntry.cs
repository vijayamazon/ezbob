using Ezbob.Logger;

namespace Ezbob.ExperianParser {
	#region class TransformationMapEntry

	class TransformationMapEntry {
		#region public

		#region constructor

		public TransformationMapEntry() {
		} // constructor

		#endregion constructor

		public string From { get; set; }
		public string To { get; set; }

		#region method Validate

		public void Validate(ASafeLog log) {
			From = From ?? "";
			To = To ?? "";
		} // Validate

		#endregion method Validate

		#endregion public
	} // class TransformationMapEntry

	#endregion class TransformationMapEntry
} // namespace Ezbob.ExperianParser
