namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable {
	using Ezbob.Database;
	using Ezbob.Utils.dbutils;

	class Warning : AWithModelOutputID {
		[FieldName("WarningID")]
		[PK(true)]
		public long ID { get; set; }

		[Length(LengthType.MAX)]
		public string Value { get; set; }
		[Length(255)]
		public string FeatureName { get; set; }
		[Length(255)]
		public string MinValue { get; set; }
		[Length(255)]
		public string MaxValue { get; set; }
	} // class Warning
} // namespace
