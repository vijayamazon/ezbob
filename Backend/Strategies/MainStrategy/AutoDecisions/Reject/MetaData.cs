namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject {
	using System;
	using System.Collections.Generic;

	public class MetaData {
		public MetaData() {
			ValidationErrors = new List<string>();
		} // constructor

		public string RowType { get; set; }

		public List<string> ValidationErrors { get; private set; }

		public void Validate() {
			if (string.IsNullOrWhiteSpace(RowType))
				throw new Exception("Meta data was not loaded.");
		} // Validate
	} // class MetaData
} // namespace
