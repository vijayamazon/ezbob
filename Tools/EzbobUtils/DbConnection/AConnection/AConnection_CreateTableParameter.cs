namespace Ezbob.Database {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Utils.dbutils;
	using Utils;

	public abstract partial class AConnection : IDisposable {
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T">Type of values that are converted into table rows.</typeparam>
		/// <param name="sFieldName">Parameter name (i.e. name of table being created).</param>
		/// <param name="aryValues">List of values to insert into table. Each value is converted into table row.</param>
		/// <returns></returns>
		public virtual QueryParameter CreateTableParameter<T>(string sFieldName, params T[] aryValues) {
			return CreateTableParameter<T>(sFieldName, (IEnumerable<T>)aryValues);
		} // CreateTableParameter

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T">Type of values that are converted into table rows.</typeparam>
		/// <param name="sFieldName">Parameter name (i.e. name of table being created).</param>
		/// <param name="oValues">List of values to insert into table. Each value is converted into table row.</param>
		/// <returns></returns>
		public virtual QueryParameter CreateTableParameter<T>(string sFieldName, IEnumerable<T> oValues) {
			return CreateTableParameter<T>(sFieldName, oValues, TypeUtils.GetConvertorToObjectArray(typeof(T)));
		} // CreateTableParameter

		public virtual QueryParameter CreateTableParameter<TColumnInfo, TSource>(string sFieldName, params TSource[] aryValues) {
			return CreateTableParameter<TColumnInfo>(sFieldName, aryValues, TypeUtils.GetConvertorToObjectArray(typeof(TSource)));
		} // CreateTableParameter

		public virtual QueryParameter CreateTableParameter<TColumnInfo, TSource>(string sFieldName, IEnumerable<TSource> oValues) {
			return CreateTableParameter<TColumnInfo>(sFieldName, oValues, TypeUtils.GetConvertorToObjectArray(typeof(TSource)));
		} // CreateTableParameter

		public virtual QueryParameter CreateTableParameter<TColumnInfo>(
			string sFieldName,
			IEnumerable oValues,
			Func<object, object[]> oValueToRow
		) {
			return CreateTableParameter(typeof(TColumnInfo), sFieldName, oValues, oValueToRow);
		} // CreateTableParameter

		public virtual QueryParameter CreateTableParameter(
			Type oColumnInfo,
			string sFieldName,
			IEnumerable oValues,
			Func<object, object[]> oValueToRow,
			IEnumerable<Type> oCustomTypeOrder = null
		) {
			var tbl = new DataTable();

			if (TypeUtils.IsSimpleType(oColumnInfo))
				AddColumn(tbl, oColumnInfo);
			else {
				if (oCustomTypeOrder == null) {
					PropertyTraverser.Traverse(oColumnInfo, (i, oPropertyInfo) => {
						object[] pkAttrList = oPropertyInfo.GetCustomAttributes(typeof(PKAttribute), false);

						if (pkAttrList.Length < 1) {
							AddColumn(tbl, oPropertyInfo.PropertyType);
							return;
						} // if no PK configured

						PKAttribute pk = (PKAttribute)pkAttrList[0];

						// Primary key which is identity is not inserted into output column list
						// because such field usage is intended for saving a new item but identity
						// column is filled by DB.

						if (!pk.WithIdentity)
							AddColumn(tbl, oPropertyInfo.PropertyType);
					});
				} else {
					foreach (Type t in oCustomTypeOrder)
						AddColumn(tbl, t);
				} // if
			} // if

			if (oValues != null)
				foreach (object v in oValues)
					tbl.Rows.Add(oValueToRow(v));

			return BuildTableParameter(sFieldName, tbl);
		} // CreateTableParameter
	} // AConnection
} // namespace Ezbob.Database
