namespace Ezbob.Integration.LogicalGlue.Models {
	using System.Collections.Generic;

	public class Feature {
		public long ID { get; set; }
		public bool IsActive { get; set; }
		public FeatureName InternalName { get; set; }
		public string RawName { get; set; }
		public string Alias { get; set; }
		public FeatureType FeatureType { get; set; }
		public decimal? MinValue { get; set; }
		public decimal? MaxValue { get; set; }

		public List<Category> Categories { get; set; }
	} // class Feature
} // namespace
