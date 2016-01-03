namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable {
	using Ezbob.Database;
	using Ezbob.Utils.dbutils;

	class EtlData {
		[FieldName("EtlDataID")]
		[PK(true)]
		public long ID { get; set; }

		public long ResponseID { get; set; }
		public long? EtlCodeID { get; set; }
		[Length(LengthType.MAX)]
		public string Message { get; set; }
	} // class EtlData
} // namespace
