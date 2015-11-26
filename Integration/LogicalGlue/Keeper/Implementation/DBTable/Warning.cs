namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable {
	class Warning : AWithModelOutputID {
		public string Value { get; set; }
		public string FeatureName { get; set; }
		public string MinValue { get; set; }
		public string MaxValue { get; set; }
	} // class Warning
} // namespace
