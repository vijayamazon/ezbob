namespace Ezbob.Database {
	using System;
	using System.Data;
	using System.Data.Common;

	#region class QueryParameter

	public class QueryParameter {
		public string Name { get; set; }
		public object Value { get; set; }
		public int? Size { get; set; }
		public DbType? Type { get; set; }
		public ParameterDirection Direction { get; set; }
		public DbParameter UnderlyingParameter { get; set; }
		public string UnderlyingTypeName { get; set; }

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
				return string.Format("{0} = {1}", Name, Value);

			case ParameterDirection.Output:
			case ParameterDirection.ReturnValue:
				return string.Format("{0} {1}", Name, Direction);

			case ParameterDirection.InputOutput:
				return string.Format("{0} = {1} <bidirectional>", Name, Value);

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ToString

		public object ReturnedValue {
			get {
				return ReferenceEquals(UnderlyingParameter, null) ? null : UnderlyingParameter.Value;
			} // get
		} // ReturnedValue

		public string SafeReturnedValue {
			get { return (ReturnedValue ?? string.Empty).ToString(); } // get
		} // SafeReturnedValue
	} // class QueryParameter

	#endregion class QueryParameter
} // namespace Ezbob.Database
