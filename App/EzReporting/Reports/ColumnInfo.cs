using System;

namespace Reports {

	public enum ValueType {
		Currency,
		Percent,
		CssClass,
		ID,
		Other,
		UserID,
		BrokerID,
	} // enum ValueType

	public class ColumnInfo {

		public ColumnInfo(string sCaption, string sFieldName) {
			ValueType = ValueType.Other;

			Caption = (sCaption ?? "").Trim();
			Caption = (Caption == string.Empty) ? "&nbsp;" : Caption;

			FieldName = (sFieldName ?? "").Trim();

			if (FieldName == string.Empty)
				throw new Exception("Field name is not specified.");

			switch (FieldName[0]) {
			case '$':
				FieldName = FieldName.Substring(1);
				ValueType = ValueType.Currency;
				break;

			case '#':
				FieldName = FieldName.Substring(1);
				ValueType = ValueType.UserID;
				break;

			case '^':
				FieldName = FieldName.Substring(1);
				ValueType = ValueType.BrokerID;
				break;

			case '!':
				FieldName = FieldName.Substring(1);
				ValueType = ValueType.ID;
				break;

			case '%':
				FieldName = FieldName.Substring(1);
				ValueType = ValueType.Percent;
				break;

			case '{':
				FieldName = FieldName.Substring(1);
				ValueType = ValueType.CssClass;
				break;
			} // if

			if (FieldName == string.Empty)
				throw new Exception("Field name is not specified.");
		} // constructor

		public string Caption { get; private set; }

		public string FieldName { get; private set; }

		public ValueType ValueType { get; private set; }

		public bool IsVisible {
			get { return ValueType != ValueType.CssClass; }
		} // IsVisible

		public string Format(int nPrecision) {
			switch (ValueType) {
			case ValueType.Currency:
				return "C" + nPrecision;

			case ValueType.UserID:
			case ValueType.ID:
			case ValueType.BrokerID:
				return "G" + nPrecision;

			case ValueType.Percent:
				return "P" + nPrecision;

			case ValueType.CssClass:
			case ValueType.Other:
				return "N" + nPrecision;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // Format

	} // class ColumnInfo

} // namespace Reports
