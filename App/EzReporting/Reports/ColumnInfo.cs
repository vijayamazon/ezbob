using System;

namespace Reports {
	#region enum ValueType

	public enum ValueType {
		Currency,
		Percent,
		CssClass,
		ID,
		Other,
		UserID,
		BrokerID,
	} // enum ValueType

	#endregion enum ValueType

	#region class ColumnInfo

	public class ColumnInfo {
		#region public

		#region constructor

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

		#endregion constructor

		#region property Caption

		public string Caption { get; private set; }

		#endregion property Caption

		#region property FieldName

		public string FieldName { get; private set; }

		#endregion property FieldName

		#region property ValueType

		public ValueType ValueType { get; private set; }

		#endregion property ValueType

		#region property IsVisible

		public bool IsVisible {
			get { return ValueType != ValueType.CssClass; }
		} // IsVisible

		#endregion property IsVisible

		#region property Format

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
		#endregion property Format

		#endregion public
	} // class ColumnInfo

	#endregion class ColumnInfo
} // namespace Reports
