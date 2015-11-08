namespace Ezbob.Integration.LogicalGlue.Keeper.DBTable {
	class Warning : AWithResponseID {
		public long WarningID { get; set; }
		public string Value { get; set; }
		public string FeatureName { get; set; }
		public string MinValue { get; set; }
		public string MaxValue { get; set; }
	} // class Warning
} // namespace
