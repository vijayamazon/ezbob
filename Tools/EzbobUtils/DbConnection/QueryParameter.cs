namespace Ezbob.Database {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.Common;
	using Ezbob.Utils.Security;
	using Utils;

	public class QueryParameter {
		public string Name { get; set; }
		public int? Size { get; set; }
		public DbType? Type { get; set; }
		public ParameterDirection Direction { get; set; }
		public DbParameter UnderlyingParameter { get; set; }
		public string UnderlyingTypeName { get; set; }
		public string ObjectPropertyName { get; set; }

		public object Value {
			get { return this.parameterValue; }
			set {
				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				if (ReferenceEquals(value, null) || (value == null)) {
					this.parameterValue = DBNull.Value;
					return;
				} // if

				if (value.GetType() == typeof(Encrypted)) {
					this.parameterValue = (byte[])((Encrypted)value);
					return;
				} // if

				this.parameterValue = value;
			} // if
		} // Value

		public QueryParameter(DbParameter oUnderlyingParameter) {
			if (ReferenceEquals(oUnderlyingParameter, null))
				throw new ArgumentNullException("oUnderlyingParameter", "Underlying parameter is null.");

			UnderlyingParameter = oUnderlyingParameter;

			Name = UnderlyingParameter.ParameterName;
			Value = UnderlyingParameter.Value;
			Size = UnderlyingParameter.Size;
			Type = UnderlyingParameter.DbType;
			Direction = UnderlyingParameter.Direction;
		} // constructor

		public QueryParameter(string sName, object oValue = null) {
			Name = sName;
			Value = oValue ?? DBNull.Value;
			Direction = ParameterDirection.Input;
		} // constructor

		public override string ToString() {
			switch (Direction) {
			case ParameterDirection.Input:
				return string.Format("{0} = {1}", Name, ValueStr);

			case ParameterDirection.Output:
			case ParameterDirection.ReturnValue:
				return string.Format("{0} {1}", Name, Direction);

			case ParameterDirection.InputOutput:
				return string.Format("{0} = {1} <bidirectional>", Name, ValueStr);

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ToString

		private string ValueStr {
			get {
				if (Value == null)
					return "-- NULL --";

				if (Value is DBNull)
					return "-- DB NULL --";

				if (TypeUtils.IsSimpleType(Value.GetType()))
					return Value.ToString();

				if (Value is DataTable) {
					var tbl = Value as DataTable;

					var os = new List<string>();

					var oNames = new List<string>();

					for (int i = 0; i < tbl.Columns.Count; i++)
						oNames.Add(tbl.Columns[i].ColumnName + ": ");

					foreach (DataRow oRow in tbl.Rows) {
						var osRow = new List<string>();

						for (int i = 0; i < tbl.Columns.Count; i++) {
							object v = oRow[i];
							osRow.Add(oNames[i] + (v == null ? "-- null --" : v.ToString()));
						} // for each cell

						os.Add("{" + string.Join(", ", osRow) + "}");
					} // for each row

					return string.Format(
						"[{0}]: {{ {1} }}",
						os.Count,
						string.Join(" ; ", os)
					);
				} // if

				return "something else (neither DataTable nor simple type) of type " + Value.GetType();
			} // get
		} // ValueStr

		public object ReturnedValue {
			get {
				return ReferenceEquals(UnderlyingParameter, null) ? null : UnderlyingParameter.Value;
			} // get
		} // ReturnedValue

		public string SafeReturnedValue {
			get { return (ReturnedValue ?? string.Empty).ToString(); } // get
		} // SafeReturnedValue

		private object parameterValue;
	} // class QueryParameter
} // namespace Ezbob.Database
