namespace Ezbob.Utils.dbutils {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	public class CodeToSql {
		public static string GetCreateTable<T>() where T : class {
			var oFields = new List<string>();
			var oConstraints = new List<string>();

			string sTableName = typeof(T).Name;

			PropertyTraverser.Traverse(typeof(T), (ignored, oPropInfo) => {
				string sType = T2T(oPropInfo);

				if (string.IsNullOrWhiteSpace(sType))
					return;

				List<CustomAttributeData> oKeyAttr = oPropInfo.CustomAttributes
					.Where(a => a.AttributeType == typeof(FKAttribute) || a.AttributeType == typeof(PKAttribute))
					.ToList();

				string fullPropName = "\t\t[" + oPropInfo.Name + "] " + sType;

				if (oKeyAttr.Count == 0) {
					oFields.Add(fullPropName);
					return;
				} // if

				bool isPk = false;

				if (oKeyAttr.Any(x => x.AttributeType == typeof(PKAttribute))) {
					oConstraints.Add("\t\tCONSTRAINT PK_" + sTableName + " PRIMARY KEY ([" + oPropInfo.Name + "])");
					oFields.Insert(0, fullPropName);
					isPk = true;
				} // if

				var fk = oPropInfo.GetCustomAttribute<FKAttribute>();

				if ((fk != null) && !string.IsNullOrWhiteSpace(fk.TableName)) {
					oConstraints.Add(
						"\t\tCONSTRAINT FK_" + sTableName + "_" + oPropInfo.Name +
						" FOREIGN KEY ([" + oPropInfo.Name + "]) REFERENCES [" + fk.TableName + "] ([" + fk.FieldName + "])"
					);
				} // if

				if (!isPk)
					oFields.Add(fullPropName);
			});

			oFields.Add("\t\t[TimestampCounter] ROWVERSION");

			return
				//"SET QUOTED_IDENTIFIER ON\nGO\n\n" +
				"IF OBJECT_ID('" + sTableName + "') IS NULL\nBEGIN\n" +
				"\tCREATE TABLE [" + sTableName + "] (\n" +
				string.Join(",\n", oFields) +
				(oConstraints.Count < 1 ? "" : ",\n" + string.Join("\n", oConstraints)) +
				"\n\t)\nEND\nGO\n\n";
		} // GetCreateTable

		public static string GetCreateSp<T>() where T : class {
			var oSql = new List<string>();
			var oFields = new List<string>();
			var oFieldNames = new List<string>();

			List<string> oProcSql = new List<string>();

			string sTableName = typeof(T).Name;

			string sProcName = "Save" + sTableName;

			string sTypeName = sTableName + "List";

			oSql.Add("IF OBJECT_ID('" + sProcName + "') IS NOT NULL\n\tDROP PROCEDURE " + sProcName + "\nGO\n");

			oSql.Add("IF TYPE_ID('" + sTypeName + "') IS NOT NULL\n\tDROP TYPE " + sTypeName + "\nGO\n");

			oSql.Add("CREATE TYPE " + sTypeName + " AS TABLE (");

			PropertyTraverser.Traverse(typeof(T), (ignored, oPropInfo) => {
				string sType = T2T(oPropInfo);

				List<bool> oKeyAttr = oPropInfo.CustomAttributes
					.Where(a => a.AttributeType == typeof(PKAttribute) && a.ConstructorArguments.Count > 0)
					.Select(a => (bool)a.ConstructorArguments[0].Value)
					.ToList();

				if (oKeyAttr.Count > 0 && oKeyAttr[0]) // is identity
					return;

				if (!string.IsNullOrWhiteSpace(sType)) {
					if (oPropInfo.DeclaringType == typeof(T)) {
						oFields.Add("["+ oPropInfo.Name + "] " + sType);
						oFieldNames.Add("[" + oPropInfo.Name  + "]");
					} else {
						oSql.Add("\t[" + oPropInfo.Name + "] " + sType + ",");
						oFieldNames.Insert(0, "[" + oPropInfo.Name + "] ");
					} // if
				} // if
			});

			var sFieldNames = string.Join(",\n\t\t", oFieldNames);

			oProcSql.Add("CREATE PROCEDURE " + sProcName);
			oProcSql.Add("@Tbl " + sTypeName + " READONLY");
			oProcSql.Add("AS");
			oProcSql.Add("BEGIN");
			oProcSql.Add("\tSET NOCOUNT ON;\n");
			oProcSql.Add("\tINSERT INTO " + sTableName + " (");
			oProcSql.Add("\t\t" + sFieldNames);
			oProcSql.Add("\t) SELECT");
			oProcSql.Add("\t\t" + sFieldNames);
			oProcSql.Add("\tFROM @Tbl");
			oProcSql.Add("END");
			oProcSql.Add("GO");

			return
			 //"SET QUOTED_IDENTIFIER ON\nGO\n\n" +
			 string.Join("\n", oSql) + "\n\t" +
			 string.Join(",\n\t", oFields) + "\n)\nGO\n\n" +
			 string.Join("\n", oProcSql) + "\n\n";
		} // GetCreateSp

		private static string T2T(PropertyInfo oPropInfo) {
			if (oPropInfo.PropertyType == typeof(string))
				return "NVARCHAR(" + ExtractLength(oPropInfo, "255") + ") NULL";

			if (oPropInfo.PropertyType == typeof(int?))
				return "INT NULL";

			if (oPropInfo.PropertyType == typeof(int))
				return "INT" + ExtractIdentity(oPropInfo) + " NULL";//" NOT NULL"

			if (oPropInfo.PropertyType == typeof(long?))
				return "BIGINT NULL";

			if (oPropInfo.PropertyType == typeof(long))
				return "BIGINT" + ExtractIdentity(oPropInfo) + " NOT NULL";

			if (oPropInfo.PropertyType == typeof(decimal))
				return "DECIMAL(" + ExtractLength(oPropInfo, "18, 6") + ") NULL";//") NOT NULL";

			if (oPropInfo.PropertyType == typeof(decimal?))
				return "DECIMAL(" + ExtractLength(oPropInfo, "18, 6") + ") NULL";

			if (oPropInfo.PropertyType == typeof(DateTime?))
				return "DATETIME NULL";

			if (oPropInfo.PropertyType == typeof(DateTime))
				return "DATETIME NULL";//"DATETIME NOT NULL"

			if (oPropInfo.PropertyType == typeof(bool))
				return "BIT NULL";//"BIT NOT NULL";

			if (oPropInfo.PropertyType == typeof(bool?))
				return "BIT NULL";

			return null;
		} // T2T

		private static string ExtractIdentity(PropertyInfo oPropInfo) {
			var attr = oPropInfo.GetCustomAttribute<PKAttribute>();

			if (attr == null)
				return string.Empty;

			return attr.WithIdentity ? " IDENTITY(1, 1)" : string.Empty;
		} // ExtractLength

		private static string ExtractLength(PropertyInfo oPropInfo, string defaultLength) {
			var attr = oPropInfo.GetCustomAttribute<LengthAttribute>();

			if (attr == null)
				return defaultLength;

			return attr.Length;
		} // ExtractLength
	} // class CodeToSql
} // namespace
