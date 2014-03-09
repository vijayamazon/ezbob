namespace Ezbob.Database {
	using System.Collections.Generic;
	using System.Data;

	#region class QueryTableParameter

	public class QueryTableParameter<T> : QueryParameter {
		public QueryTableParameter(string sName, string sTypeName, IEnumerable<T> oValue) : base(sName, oValue) {
			Type = SqlDbType.Structured;
			UnderlyingTypeName = sTypeName;
		} // constructor
	} // class QueryTableParameter

	#endregion class QueryTableParameter
} // namespace Ezbob.Database
