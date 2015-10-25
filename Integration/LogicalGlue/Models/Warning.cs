namespace Ezbob.Integration.LogicalGlue.Models {
	public class Warning {
		public long ID { get; set; }

		public Response Response { get; set; }

		public Feature Feature { get; set; }
		public string Value { get; set; }

		public string FeatureName { get; set; }
		public string MinValue { get; set; }
		public string MaxValue { get; set; }
	} // class Warning
} // namespace
