namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable {
	using Ezbob.Database;
	using Ezbob.Utils.dbutils;

	class Warning : AWithModelOutputID {
		[FieldName("WarningID")]
		[PK(true)]
		public long ID { get; set; }

		public string Value { get; set; }
		public string FeatureName { get; set; }
		public string MinValue { get; set; }
		public string MaxValue { get; set; }
	} // class Warning
} // namespace
