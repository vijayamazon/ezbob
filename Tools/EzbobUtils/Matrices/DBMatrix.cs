namespace Ezbob.Matrices {
	using System;
	using Ezbob.Database;
	using Ezbob.Matrices.Core;

	public class DBMatrix : DecimalIntervalMatrix {
		public DBMatrix(string matrixName, AConnection db) {
			this.db = db;
			MatrixName = matrixName;
		} // constructor

		public virtual bool Load() {
			this.db.ForEachRowSafe(
				ProcessRow,
				"LoadMatrix",
				CommandSpecies.StoredProcedure,
				new QueryParameter("MatrixName", MatrixName)
			);

			return (MatrixID > 0) && Init();
		} // Load

		public long MatrixID { get; private set; }

		public string MatrixName { get; private set; }

		protected virtual AConnection DB { get { return this.db; } }

		protected virtual void ProcessRow(SafeReader sr) {
			RowTypes rt;

			if (!Enum.TryParse(sr["RowType"], true, out rt))
				return;

			switch (rt) {
			case RowTypes.MetaData:
				MatrixID = sr["MatrixID"];
				SetMinRowTitleValue(sr["MinRowTitleValue"]);
				SetMinColumnTitleValue(sr["MinColumnTitleValue"]);
				break;

			case RowTypes.Value:
				AddValue(sr["RowTitle"], sr["ColumnTitle"], sr["CellValue"]);
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ProcessRow

		private enum RowTypes {
			MetaData,
			Value,
		} // enum RowTypes

		private readonly AConnection db;
	} // class DBMatrix
} // namespace
