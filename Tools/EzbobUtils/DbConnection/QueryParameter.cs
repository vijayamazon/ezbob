namespace Ezbob.Database {
	using System.Data;
	using System;

	#region struct QueryParameter

	public class QueryParameter {
		public string Name { get; set; }
		public object Value { get; set; }
		public int? Size { get; set; }
		public SqlDbType? Type { get; set; }

		public QueryParameter(string sName, object oValue) {
			Name = sName;
			Value = oValue ?? DBNull.Value;
		} // constructor

		public override string ToString() {
			return string.Format("{0} = {1}", Name, Value);
		} // ToString
	} // class QueryParameter

	#endregion struct QueryParameter
} // namespace Ezbob.Database
