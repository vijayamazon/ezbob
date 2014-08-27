using System;
using System.Collections.Generic;
using System.Linq;

namespace Ezbob.Utils.dbutils
{
	using System.Reflection;

	public class CodeToSql
	{
		public static string GetCreateTable<T>() where T : class
		{
			var oFields = new List<string>();
			var oConstraints = new List<string>();

			string sTableName = typeof(T).Name;

			PropertyTraverser.Traverse(typeof(T), (oInstance, oPropInfo) =>
			{
				string sType = T2T(oPropInfo);

				if (string.IsNullOrWhiteSpace(sType))
					return;

				List<CustomAttributeData> oKeyAttr = oPropInfo.CustomAttributes
				 .Where(a => a.AttributeType == typeof(FKAttribute) || a.AttributeType == typeof(PKAttribute))
				 .ToList();

				if (oKeyAttr.Count == 0)
				{
					oFields.Add("\t\t" + oPropInfo.Name + " " + sType);
					return;
				} // if

				if (oKeyAttr.Count > 1)
					throw new Exception("A field cannot be both PRIMARY KEY and FOREIGN KEY simultaneously.");

				if (oKeyAttr.Any(x => x.AttributeType ==  typeof(PKAttribute)))
					oConstraints.Add("\t\tCONSTRAINT PK_" + sTableName + " PRIMARY KEY (" + oPropInfo.Name + ")");
				else
				{
					var fk = oPropInfo.GetCustomAttribute<FKAttribute>();

					if (!string.IsNullOrWhiteSpace(fk.TableName))
					{
						oConstraints.Add(
						 "\t\tCONSTRAINT FK_" + sTableName + "_" + oPropInfo.Name +
						  " FOREIGN KEY (" + oPropInfo.Name + ") REFERENCES " + fk.TableName + "(" + fk.FieldName + ")"
						);
					} // if
				}

				oFields.Insert(0, "\t\t" + oPropInfo.Name + " " + sType);
			});

			oFields.Add("\t\tTimestampCounter ROWVERSION");

			return
			 "SET QUOTED_IDENTIFIER ON\nGO\n\n" +
			 "IF OBJECT_ID('" + sTableName + "') IS NULL\nBEGIN\n" +
			 "\tCREATE TABLE " + sTableName + " (\n" +
			 string.Join(",\n", oFields) +
			 (oConstraints.Count < 1 ? "" : ",\n" + string.Join("\n", oConstraints)) +
			 "\n\t)\nEND\nGO\n\n";
		} // GetCreateTable

		public static string GetCreateSp<T>() where T : class
		{
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

			PropertyTraverser.Traverse(typeof(T), (oInstance, oPropInfo) =>
			{
				string sType = T2T(oPropInfo);

				if (!string.IsNullOrWhiteSpace(sType))
				{
					if (oPropInfo.DeclaringType == typeof(T))
					{
						oFields.Add(oPropInfo.Name + " " + sType);
						oFieldNames.Add(oPropInfo.Name);
					}
					else
					{
						oSql.Add("\t" + oPropInfo.Name + " " + sType + ",");
						oFieldNames.Insert(0, oPropInfo.Name);
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
			 "SET QUOTED_IDENTIFIER ON\nGO\n\n" +
			 string.Join("\n", oSql) + "\n\t" +
			 string.Join(",\n\t", oFields) + "\n)\nGO\n\n" +
			 string.Join("\n", oProcSql) + "\n\n";
		} // GetCreateSp



		private static string T2T(PropertyInfo oPropInfo)
		{
			if (oPropInfo.PropertyType == typeof(string))
				return "NVARCHAR(255) NULL";

			if (oPropInfo.PropertyType == typeof(int?))
				return "INT NULL";
			
			if (oPropInfo.PropertyType == typeof(int))
				return "INT NOT NULL";
			
			if (oPropInfo.PropertyType == typeof(long?))
				return "BIGINT NULL";

			if (oPropInfo.PropertyType == typeof(long))
				return "BIGINT NOT NULL";

			if (oPropInfo.PropertyType == typeof(decimal))
				return "DECIMAL(18,6) NOT NULL";

			if (oPropInfo.PropertyType == typeof(decimal?))
				return "DECIMAL(18, 6) NULL";

			if (oPropInfo.PropertyType == typeof(DateTime?))
				return "DATETIME NULL";

			if (oPropInfo.PropertyType == typeof(DateTime))
				return "DATETIME NOT NULL";

			if (oPropInfo.PropertyType == typeof(bool))
				return "BIT NOT NULL";

			if (oPropInfo.PropertyType == typeof(bool?))
				return "BIT NULL";
			return null;
		} // T2T
	}
}
