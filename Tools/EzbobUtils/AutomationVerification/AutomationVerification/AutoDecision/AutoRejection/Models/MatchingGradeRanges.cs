namespace AutomationCalculator.AutoDecision.AutoRejection.Models {
	using System.Collections.Generic;

	public class MatchingGradeRanges : List<MatchingGradeRanges.SubproductGradeRange> {
		public class SubproductGradeRange {
			public int ProductSubTypeID { get; set; }
			public int GradeRangeID { get; set; }

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>
			/// A string that represents the current object.
			/// </returns>
			public override string ToString() {
				return string.Format("product subtype id: {0} - grade range id: {1}", ProductSubTypeID, GradeRangeID);
			} // ToString
		} // class SubproductGradeRange
	} // class MatchingGradeRanges
} // namespace
