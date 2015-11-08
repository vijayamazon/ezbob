namespace Ezbob.Integration.LogicalGlue.Keeper.DBTable {
	class MissingColumn : AWithResponseID {
		public long MissingColumnID { get; set; }
		public string ColumnName { get; set; }
	} // class MissingColumn
} // namespace
