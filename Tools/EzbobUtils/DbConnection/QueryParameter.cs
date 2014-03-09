namespace Ezbob.Database {
	using System;
	using System.Data;
	using System.Data.Common;

	#region class QueryParameter

	public class QueryParameter {
		public virtual string Name { get; set; }
		public virtual object Value { get; set; }
		public virtual int? Size { get; set; }
		public virtual SqlDbType? Type { get; set; }
		public virtual ParameterDirection Direction { get; set; }
		public virtual DbParameter UnderlyingParameter { get; set; }
		public virtual string UnderlyingTypeName { get; set; }

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

		public virtual object ReturnedValue {
			get {
				return ReferenceEquals(UnderlyingParameter, null) ? null : UnderlyingParameter.Value;
			} // get
		} // ReturnedValue

		public virtual string SafeReturnedValue {
			get { return (ReturnedValue ?? string.Empty).ToString(); } // get
		} // SafeReturnedValue
	} // class QueryParameter

	#endregion class QueryParameter
} // namespace Ezbob.Database
