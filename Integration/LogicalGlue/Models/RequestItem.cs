namespace Ezbob.Integration.LogicalGlue.Models {
	using System;

	public class RequestItem {
		public long ID { get; set; }
		public Feature Feature { get; set; }
		public string Name { get; set; }
		public Type ValueType { get; set; }
		public string Value { get; set; }
	} // class RequestItem
} // namespace
