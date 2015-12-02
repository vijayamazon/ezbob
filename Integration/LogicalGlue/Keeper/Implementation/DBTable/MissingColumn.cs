namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable {
	using Ezbob.Database;
	using Ezbob.Utils.dbutils;

	class MissingColumn : AWithModelOutputID {
		[FieldName("MissingColumnID")]
		[PK(true)]
		public long ID { get; set; }

		[Length(255)]
		public string ColumnName { get; set; }
	} // class MissingColumn
} // namespace
