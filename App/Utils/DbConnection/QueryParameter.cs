using System.Data;

namespace Ezbob.Database {
	#region struct QueryParameter

	public class QueryParameter {
		public string Name { get; set; }
		public object Value { get; set; }
		public int? Size { get; set; }
		public SqlDbType? Type { get; set; }

		public QueryParameter(string sName, object oValue) {
			Name = sName;
			Value = oValue;
		} // constructor

		public override string ToString() {
			return string.Format("{0} = {1}", Name, Value);
		} // ToString
	} // class QueryParameter

	#endregion struct QueryParameter
} // namespace Ezbob.Database
