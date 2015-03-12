namespace Ezbob.Matrices {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Utils;

	using DecimalIntervalMatrixColumn = System.Collections.Generic.SortedDictionary<DecimalIntervalMatrixInterval, decimal?>;

	public class DecimalIntervalMatrix {
		public DecimalIntervalMatrix() {
			this.isInitialized = false;
			this.rawData = new SortedTable<DecimalIntervalMatrixIntervalEdge, DecimalIntervalMatrixIntervalEdge, decimal?>();
			this.data = new SortedDictionary<DecimalIntervalMatrixInterval, DecimalIntervalMatrixColumn>();
		} // constructor

		public virtual decimal? this[decimal rowValeRaw, decimal columnValeRaw] {
			get {
				if (!this.isInitialized)
					throw new Exception("Matrix was not initialized");

				var rowValue = new DecimalIntervalMatrixIntervalEdge(rowValeRaw);

				DecimalIntervalMatrixColumn column = null;

				foreach (KeyValuePair<DecimalIntervalMatrixInterval, DecimalIntervalMatrixColumn> pair in this.data) {
					var rowInterval = pair.Key;

					if (!rowInterval.Contains(rowValue))
						continue;

					column = pair.Value;
					break;
				} // for each

				if (column == null)
					return null;

				var columnValue = new DecimalIntervalMatrixIntervalEdge(columnValeRaw);

				foreach (KeyValuePair<DecimalIntervalMatrixInterval, decimal?> pair in column) {
					if (pair.Key.Contains(columnValue))
						return pair.Value;
				} // for each

				return null;
			} // get
		} // indexer

		public virtual decimal? GetItem(decimal rowValue, decimal columnValue) {
			return this[rowValue, columnValue];
		} // GetItem

		public string ToFormattedString() {
			var tbl = new SortedTable<string, string, string>();

			foreach (var pair in this.data) {
				string key = pair.Key.ToString();

				foreach (var pp in pair.Value) {
					tbl[key, pp.Key.ToString()] = pp.Value == null
						? "- null -"
						: pp.Value.Value.ToString("N6", CultureInfo.InvariantCulture);
				} // for each column
			} // for each

			return tbl.ToFormattedString();
		} // ToFormattedString

		protected virtual void AddValue(decimal? rowTitleValueRaw, decimal? columnTitleValueRaw, decimal? cellValue) {
			DecimalIntervalMatrixIntervalEdge rowTitleValue = rowTitleValueRaw == null
				? new DecimalIntervalMatrixIntervalEdge(true)
				: new DecimalIntervalMatrixIntervalEdge(rowTitleValueRaw.Value);

			if (rowTitleValue <= MinRowTitleValue)
				return;

			DecimalIntervalMatrixIntervalEdge columnTitleValue = columnTitleValueRaw == null
				? new DecimalIntervalMatrixIntervalEdge(true)
				: new DecimalIntervalMatrixIntervalEdge(columnTitleValueRaw.Value);

			if (columnTitleValue <= MinColumnTitleValue)
				return;

			this.rawData[rowTitleValue, columnTitleValue] = cellValue;
		} // AddValue

		protected virtual void Init() {
			this.isInitialized = false;
			this.data.Clear();

			if (!this.rawData.HasAlignedColumns())
				return;

			DecimalIntervalMatrixIntervalEdge prevRowTitle = MinRowTitleValue;

			this.rawData.ForEachRow((curRowTitle, curRow) => {
				DecimalIntervalMatrixIntervalEdge prevColumnTitle = MinColumnTitleValue;

				var column = new DecimalIntervalMatrixColumn();

				this.data[new DecimalIntervalMatrixInterval(prevRowTitle, curRowTitle)] = column;

				foreach (var pair in curRow) {
					var curColumnTitle = pair.Key;

					column[new DecimalIntervalMatrixInterval(prevColumnTitle, curColumnTitle)] = pair.Value;

					prevColumnTitle = curColumnTitle;
				} // for each column

				prevRowTitle = curRowTitle;
			});

			this.rawData.Clear();
			this.isInitialized = true;
		} // Init

		protected virtual void SetMinRowTitleValue(decimal? v) {
			this.minRowTitleValue = v == null
				? new DecimalIntervalMatrixIntervalEdge(false)
				: new DecimalIntervalMatrixIntervalEdge(v.Value);
		} // SetMinRowTitleValue

		protected virtual void SetMinColumnTitleValue(decimal? v) {
			this.minColumnTitleValue = v == null
				? new DecimalIntervalMatrixIntervalEdge(false)
				: new DecimalIntervalMatrixIntervalEdge(v.Value);
		} // SetMinColumnTitleValue

		private DecimalIntervalMatrixIntervalEdge MinRowTitleValue {
			get {
				if (this.minRowTitleValue == null)
					SetMinRowTitleValue(0);

				return this.minRowTitleValue;
			} // get
		} // MinRowTitleValue

		private DecimalIntervalMatrixIntervalEdge MinColumnTitleValue {
			get {
				if (this.minColumnTitleValue == null)
					SetMinColumnTitleValue(0);

				return this.minColumnTitleValue;
			} // get
		} // MinColumnTitleValue

		private DecimalIntervalMatrixIntervalEdge minRowTitleValue;
		private DecimalIntervalMatrixIntervalEdge minColumnTitleValue;

		private readonly SortedDictionary<DecimalIntervalMatrixInterval, DecimalIntervalMatrixColumn> data;

		private readonly SortedTable<DecimalIntervalMatrixIntervalEdge, DecimalIntervalMatrixIntervalEdge, decimal?> rawData;

		private bool isInitialized;
	} // class DecimalIntervalMatrix
} // namespace
