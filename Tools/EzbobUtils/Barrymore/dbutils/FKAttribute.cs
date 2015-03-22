using System;

namespace Ezbob.Utils.dbutils {
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class FKAttribute : Attribute {
		public FKAttribute(string sTableName = null, string sFieldName = null) {
			if (string.IsNullOrWhiteSpace(sTableName) || string.IsNullOrWhiteSpace(sFieldName)) {
				TableName = null;
				FieldName = null;
			} else {
				TableName = sTableName;
				FieldName = sFieldName;
			} // if
		} // constructor

		public string TableName { get; private set; }
		public string FieldName { get; private set; }
	} // class FKAttribute
}
