﻿namespace Ezbob.Database {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.Common;
	using System.Linq;
	using System.Reflection;
	using Ezbob.Utils;
	using Ezbob.Logger;

	#region class DirectionAttribute

	[System.AttributeUsage(
		System.AttributeTargets.Property,
		AllowMultiple = false
	)]
	public class DirectionAttribute : Attribute {
		#region constructor

		public DirectionAttribute(ParameterDirection nDirection = ParameterDirection.Input) {
			Direction = nDirection;
		} // constructor

		#endregion constructor

		#region property Direction

		public ParameterDirection Direction { get; private set; } // Direction

		#endregion property Direction
	} // class DirectionAttribute

	#endregion class DirectionAttribute

	#region class AStoredProcedure

	public abstract class AStoredProcedure : ITraversable, IStoredProcedure {
		#region public

		public abstract bool HasValidParameters();

		#region method ExecuteScalar

		public virtual T ExecuteScalar<T>() {
			if (!IsReadyToGo())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			return DB.ExecuteScalar<T>(GetName(), Species, PrepareParameters());
		} // ExecuteScalar

		#endregion method ExecuteScalar

		#region method ExecuteReader

		public virtual DataTable ExecuteReader() {
			if (!IsReadyToGo())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			return DB.ExecuteReader(GetName(), Species, PrepareParameters());
		} // ExecuteReader

		#endregion method ExecuteReader

		#region method ExecuteNonQuery

		public virtual int ExecuteNonQuery() {
			if (!IsReadyToGo())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			return DB.ExecuteNonQuery(GetName(), Species, PrepareParameters());
		} // ExecuteNonQuery

		#endregion method ExecuteNonQuery

		#region method ForEachRow

		public virtual void ForEachRow(Func<DbDataReader, bool, ActionResult> oAction) {
			if (!IsReadyToGo())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			DB.ForEachRow(oAction, GetName(), Species, PrepareParameters());
		} // ForEachRow

		#endregion method ForEachRow

		#region method ForEachRowSafe

		public virtual void ForEachRowSafe(Func<SafeReader, bool, ActionResult> oAction) {
			if (!IsReadyToGo())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			DB.ForEachRowSafe(oAction, GetName(), Species, PrepareParameters());
		} // ForEachRowSafe

		#endregion method ForEachRowSafe

		#region method ForEachResult

		public virtual void ForEachResult(Func<IResultRow, ActionResult> oAction) {
			if (!IsReadyToGo())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			if (oAction == null)
				throw new ArgumentNullException("oAction", "No action specified for 'ForEachResult' call.");

			var oResultRowType = GetType().GetNestedType("ResultRow", BindingFlags.Public);

			if (oResultRowType == null)
				throw new NotImplementedException("This class does not have a nested public ResultRow class.");

			if (null == oResultRowType.GetInterface(typeof (IResultRow).ToString()))
				throw new NotImplementedException("Nested ResultRow class does not implement " + typeof (IResultRow));

			var oConstructorInfo = oResultRowType.GetConstructors().FirstOrDefault(ci => ci.GetParameters().Length == 0);

			if (oConstructorInfo == null)
				throw new NotImplementedException("Nested ResultRow class has no parameterless constructor.");

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					var oRow = (IResultRow)oConstructorInfo.Invoke(null);
					oRow.SetIsFirst(bRowsetStart);

					sr.Fill(oRow);

					return oAction(oRow);
				},
				GetName(),
				Species,
				PrepareParameters()
			);
		} // ForEachResult

		public virtual void ForEachResult<T>(Func<T, ActionResult> oAction) where T: IResultRow, new() {
			if (!IsReadyToGo())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			DB.ForEachResult(oAction, GetName(), Species, PrepareParameters());
		} // ForEachResult

		#endregion method ForEachResult

		#region method Fill

		public virtual List<T> Fill<T>() where T : ITraversable, new() {
			if (!IsReadyToGo())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			return DB.Fill<T>(GetName(), Species, PrepareParameters());
		} // Fill

		#endregion method Fill

		#region method FillFirst

		public virtual T FillFirst<T>() where T: ITraversable, new() {
			if (!IsReadyToGo())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			return DB.FillFirst<T>(GetName(), Species, PrepareParameters());
		} // FillFirst

		public virtual void FillFirst<T>(T oInstance) where T : ITraversable {
			if (!IsReadyToGo())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			DB.FillFirst(oInstance, GetName(), Species, PrepareParameters());
		} // FillFirst

		#endregion method FillFirst

		#region method ToString

		public override string ToString() {
			return string.Format("{0} {1}", Species, GetName());
		} // ToString

		#endregion method ToString

		#endregion public

		#region protected

		#region constructor

		protected AStoredProcedure(ASafeLog oLog, CommandSpecies nSpecies = CommandSpecies.StoredProcedure) : this(null, oLog, nSpecies) {
		} // constructor

		protected AStoredProcedure(AConnection oDB, ASafeLog oLog = null, CommandSpecies nSpecies = CommandSpecies.StoredProcedure) {
			m_aryArgs = null;
			Log = new SafeLog(oLog);
			Species = nSpecies;

			CheckDirection();

			DB = oDB;
		} // constructor

		#endregion constructor

		#region method IsReadyToGo

		protected virtual bool IsReadyToGo() {
			if (DB == null)
				throw new ArgumentNullException("Database connection not specified for " + this, (Exception)null);

			return HasValidParameters();
		} // IsReadyToGo

		#endregion method IsReadyToGo

		#region method CheckDirection

		protected virtual void CheckDirection() {
			bool bReturnFound = false;

			this.Traverse((oInstance, oPropertyInfo) => {
				object[] oAttrList = oPropertyInfo.GetCustomAttributes(typeof(DirectionAttribute), false);

				if (oAttrList.Length < 1)
					return;

				var oAttr = (DirectionAttribute)oAttrList[0];

				if (oAttr.Direction == ParameterDirection.ReturnValue) {
					if (bReturnFound)
						throw new DbException("Multiple 'return value' parameters configured for stored procedure " + GetName());

					bReturnFound = true;
				} // if
			});
		} // CheckDirection

		#endregion method CheckDirection

		#region property DB

		protected virtual AConnection DB { get; set; } // DB

		#endregion property DB

		#region property Log

		protected virtual ASafeLog Log { get; private set; } // Log

		#endregion property Log

		#region property Species

		protected virtual CommandSpecies Species { get; private set; }

		#endregion property Species

		#region method PrepareCustomParameters

		protected virtual void PrepareCustomParameters(List<QueryParameter> args) {
			// Nothing here.
			// In derived class: manipulate the args list: add, remove, do something non-standard.
		} // PrepareCustomParameters

		#endregion method PrepareCustomParameters

		#region method PrepareParameters

		protected virtual QueryParameter[] PrepareParameters() {
			if (m_aryArgs == null) {
				var args = new List<QueryParameter>();

				this.Traverse((oInstance, oPropertyInfo) => {
					object[] oNameAttrList = oPropertyInfo.GetCustomAttributes(typeof (FieldNameAttribute), false);

					string sFieldName = (oNameAttrList.Length > 0) ? ((FieldNameAttribute)oNameAttrList[0]).Name : oPropertyInfo.Name;

					object[] oDirAttrList = oPropertyInfo.GetCustomAttributes(typeof (DirectionAttribute), false);

					ParameterDirection nDirection = (oDirAttrList.Length > 0) ? ((DirectionAttribute)oDirAttrList[0]).Direction : ParameterDirection.Input;

					QueryParameter qp;

					bool bIsByteArray = oPropertyInfo.PropertyType == typeof (byte[]);

					bool bIsSimpleType = !bIsByteArray && (
						(oPropertyInfo.PropertyType == typeof (string)) ||
						(null == oPropertyInfo.PropertyType.GetInterface(typeof (IEnumerable).ToString()))
					);

					if (bIsByteArray) {
						qp = new QueryParameter(sFieldName, oPropertyInfo.GetValue(oInstance, null)) {
							Direction = nDirection,
							Type = DbType.Binary,
						};
					}
					else if (bIsSimpleType) {
						qp = new QueryParameter(sFieldName, oPropertyInfo.GetValue(oInstance, null)) {
							Direction = nDirection,
						};
					}
					else {
						if (TypeUtils.IsEnumerable(oPropertyInfo.PropertyType)) {
							Type oUnderlyingType = oPropertyInfo.PropertyType.GetGenericArguments()[0];

							qp = DB.CreateTableParameter(
								oUnderlyingType,
								sFieldName,
								(IEnumerable)oPropertyInfo.GetValue(oInstance, null),
								TypeUtils.GetConvertorToObjectArray(oUnderlyingType),
								nDirection
							);
						}
						else
							throw new NotImplementedException("Type " + oPropertyInfo.PropertyType + " does not implement IEnumerable<T>");
					} // if

					args.Add(qp);
				});

				PrepareCustomParameters(args);

				m_aryArgs = args.ToArray();
			} // if

			return m_aryArgs;
		} // PrepareParameters

		#endregion method PrepareParameters

		#region method GetName

		/// <summary>
		/// Returns the name of the stored procedure.
		/// Stored procedure name is current class name.
		/// </summary>
		/// <returns>SP name.</returns>
		protected virtual string GetName() {
			return this.GetType().Name;
		} // GetName

		#endregion method GetName

		#endregion protected

		#region private

		private QueryParameter[] m_aryArgs;

		#endregion private
	} // class AStoredProcedure

	#endregion class AStoredProcedure

	#region class AStoredProc

	public abstract class AStoredProc : AStoredProcedure {
		#region constructor

		protected AStoredProc(ASafeLog oLog, CommandSpecies nSpecies = CommandSpecies.StoredProcedure) : this(null, oLog, nSpecies) {
		} // constructor

		protected AStoredProc(AConnection oDB, ASafeLog oLog = null, CommandSpecies nSpecies = CommandSpecies.StoredProcedure) : base(oDB, oLog, nSpecies) {
		} // constructor

		#endregion constructor

		#region method GetName

		/// <summary>
		/// Returns the name of the stored procedure.
		/// Stored procedure name is current class name.
		/// If current class name starts with SP (case insensitive) these two letters are dropped
		/// (i.e. SpDoSomething -> DoSomething).
		/// If current class name is 'SP' (case insensitive) it is used as a stored procedure name.
		/// </summary>
		/// <returns>SP name.</returns>
		protected override string GetName() {
			string sName = this.GetType().Name;

			if (sName.Equals("sp", StringComparison.InvariantCultureIgnoreCase))
				return sName;

			if (sName.StartsWith("sp", StringComparison.InvariantCultureIgnoreCase))
				sName = sName.Substring(2);

			return sName;
		} // GetName

		#endregion method GetName
	} // class AStoredProc

	#endregion class AStoredProc
} // namespace Ezbob.Database
