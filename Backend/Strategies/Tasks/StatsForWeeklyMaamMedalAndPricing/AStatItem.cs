namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using System.Drawing;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using OfficeOpenXml;
	using OfficeOpenXml.Style;

	internal abstract class AStatItem {
		public abstract void Add(Datum d, int cashRequestIndex);

		public virtual int ToXlsx(int row) {
			bool showTitle = true;

			var countValues = PrepareCountRowValues();

			if (countValues != null) {
				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				row = SetRowValues(row, showTitle, countValues);
				showTitle = false;
			} else {
				List<TitledValue[]> multiCountValues = PrepareMultipleCountRowValues();

				if (multiCountValues != null) {
					foreach (TitledValue[] cv in multiCountValues) {
						row = SetRowValues(row, showTitle, cv);
						showTitle = false;
					} // for
				} // if
			} // if

			TitledValue[] amountValues = PrepareAmountRowValues();

			if (amountValues != null) {
				if (amountValues.Length > 0) {
					row = SetRowValues(row, showTitle, amountValues);
					showTitle = false;
				} // if
			} else {
				List<TitledValue[]> multiAmountValues = PrepareMultipleAmountRowValues();
				
				if (multiAmountValues != null) {
					foreach (TitledValue[] cv in multiAmountValues) {
						if ((cv != null) && (cv.Length > 0)) {
							row = SetRowValues(row, showTitle, cv);
							showTitle = false;
						} // if
					} // for
				} // if
			} // if

			return row;
		} // ToXlsx

		public static int LastColumnNumber {
			get { return MaxDataItemsCount * 2 + 1; }
		} // LastColumnNumber

		public const int MaxDataItemsCount = 6;

		public static ExcelRange SetBorder(ExcelRange range) {
			range.Style.Border.BorderAround(ExcelBorderStyle.Thin, BorderColor);
			return range;
		} // SetBorder

		public ASafeLog Log { get; private set; }

		protected AStatItem(ASafeLog log, ExcelWorksheet sheet, string title, params AStatItem[] superior) {
			Log = log.Safe();
			this.sheet = sheet;
			this.title = title;
			Count = 0;
			Amount = 0;
			LastWasAdded = false;
			LastAmount = 0;
			Added = new AddTrigger(this);
			Superior = new List<AStatItem>(superior);
		} // constructor

		protected internal virtual int Count { get; private set; }

		protected internal virtual decimal Amount { get; private set; }

		protected internal virtual bool LastWasAdded { get; private set; }

		protected internal virtual decimal LastAmount { get; private set; }

		protected internal virtual AddTrigger Added { get; private set; }

		protected internal class AddTrigger {
			public AddTrigger(AStatItem item) {
				this.item = item;
			} // AddTrigger

			public bool If(bool added, decimal amount = 0, int count = 1) {
				return added ? Yes(amount, count) : No();
			} // If

			public bool Yes(decimal amount = 0, int count = 1) {
				this.item.Amount += amount;
				this.item.LastAmount = amount;

				this.item.Count += count;
				this.item.LastWasAdded = true;
				return true;
			} // Yes

			public bool No() {
				this.item.LastAmount = 0;
				this.item.LastWasAdded = false;
				return false;
			} // No

			private readonly AStatItem item;
		} // AddTrigger

		protected internal virtual List<AStatItem> Superior { get; private set; }

		protected abstract TitledValue[] PrepareCountRowValues();

		protected virtual List<TitledValue[]> PrepareMultipleCountRowValues() {
			return new List<TitledValue[]>();
		} // PrepareMultipleCountRowValues

		protected virtual TitledValue[] PrepareAmountRowValues() {
			return new TitledValue[0];
		} // PrepareAmountRowValues

		protected virtual List<TitledValue[]> PrepareMultipleAmountRowValues() {
			return new List<TitledValue[]>();
		} // PrepareMultipleAmountRowValues

		protected virtual int SetRowValues(int row, bool showTitle, params TitledValue[] values) {
			SetBorder(this.sheet.Cells[row, 1]);
			this.sheet.SetCellValue(row, 1, showTitle ? this.title : string.Empty, true);
			return SetRowValues(row, values);
		} // SetRowValues

		protected virtual int SetRowValues(int row, string rowTitle, params TitledValue[] values) {
			SetBorder(this.sheet.Cells[row, 1]);
			this.sheet.SetCellValue(row, 1, rowTitle, true);
			return SetRowValues(row, values);
		} // SetRowValues

		protected virtual int SetRowValues(int row, params TitledValue[] values) {
			int column = 2;

			for (int i = 0; i < values.Length; i++) {
				SetOneValue(row, column, values[i]);
				column += 2;
			} // for

			for (; column <= LastColumnNumber; column += 2)
				SetOneValue(row, column, TitledValue.Default);

			return row + 1;
		} // SetRowValues

		private void SetOneValue(int row, int column, TitledValue val) {
			this.sheet.SetCellValue(row, column, val.Title, sNumberFormat: val.TitleFormat);
			this.sheet.SetCellValue(row, column + 1, val.Value, true, sNumberFormat: val.ValueFormat);

			SetBorder(this.sheet.Cells[row, column]).Style.Border.Right.Style = ExcelBorderStyle.None;
			SetBorder(this.sheet.Cells[row, column + 1]).Style.Border.Left.Style = ExcelBorderStyle.None;
		} // SetOneValue

		private readonly ExcelWorksheet sheet;
		private readonly string title;

		private static readonly Color BorderColor = Color.Black;
	} // class AStatItem
} // namespace
