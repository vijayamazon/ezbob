namespace Ezbob.Backend.ModelsWithDB {
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	[DataContract]
	public class MultiBrandLoanSummary {
		public MultiBrandLoanSummary() {
			Loans = new List<string>();
		} // constructor

		[DataMember]
		public int OriginCount { get; set; }

		[DataMember]
		public List<string> Loans { get; set; }
	} // class MultiBrandLoanSummary
} // namespace
