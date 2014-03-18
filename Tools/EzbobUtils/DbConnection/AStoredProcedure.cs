namespace Ezbob.Database {
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
			if (!HasValidParameters())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			return DB.ExecuteScalar<T>(GetName(), Species, PrepareParameters());
		} // ExecuteScalar

		#endregion method ExecuteScalar

		#region method ExecuteReader

		public virtual DataTable ExecuteReader() {
			if (!HasValidParameters())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			return DB.ExecuteReader(GetName(), Species, PrepareParameters());
		} // ExecuteReader

		#endregion method ExecuteReader

		#region method ExecuteNonQuery

		public virtual int ExecuteNonQuery() {
			if (!HasValidParameters())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			return DB.ExecuteNonQuery(GetName(), Species, PrepareParameters());
		} // ExecuteNonQuery

		#endregion method ExecuteNonQuery

		#region method ForEachRow

		public virtual void ForEachRow(Func<DbDataReader, bool, ActionResult> oAction) {
			if (!HasValidParameters())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			DB.ForEachRow(oAction, GetName(), Species, PrepareParameters());
		} // ForEachRow

		#endregion method ForEachRow

		#region method ForEachRowSafe

		public virtual void ForEachRowSafe(Func<SafeReader, bool, ActionResult> oAction) {
			if (!HasValidParameters())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			DB.ForEachRowSafe(oAction, GetName(), Species, PrepareParameters());
		} // ForEachRowSafe

		#endregion method ForEachRowSafe

		#region method ForEachResult

		public virtual void ForEachResult(Func<IResultRow, ActionResult> oAction) {
			if (!HasValidParameters())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			if (oAction == null)
				throw new ArgumentNullException("oAction", "No action specified for 'ForEachResult' call.");

			var oResultRowType = this.GetType().GetNestedType("ResultRow", BindingFlags.Public);

			if (oResultRowType == null)
				throw new NotImplementedException("This class does not have a nested public ResultRow class.");

			if (null == oResultRowType.GetInterface(typeof (IResultRow).ToString()))
				throw new NotImplementedException("Nested ResultRow class does not implement " + typeof (IResultRow).ToString());

			var oConstructorInfo = oResultRowType.GetConstructors().FirstOrDefault(ci => ci.GetParameters().Length == 0);

			if (oConstructorInfo == null)
				throw new NotImplementedException("Nested ResultRow class has no parameterless constructor.");

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					IResultRow oRow = (IResultRow)oConstructorInfo.Invoke(null);
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
			if (!HasValidParameters())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			DB.ForEachResult<T>(oAction, GetName(), Species, PrepareParameters());
		} // ForEachResult

		#endregion method ForEachResult

		#region method Fill

		public virtual List<T> Fill<T>() where T : ITraversable, new() {
			if (!HasValidParameters())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			return DB.Fill<T>(GetName(), Species, PrepareParameters());
		} // Fill

		#endregion method Fill

		#region method FillFirst

		public virtual T FillFirst<T>() where T: ITraversable, new() {
			if (!HasValidParameters())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			return DB.FillFirst<T>(GetName(), Species, PrepareParameters());
		} // FillFirst

		public virtual void FillFirst<T>(T oInstance) where T : ITraversable {
			if (!HasValidParameters())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			DB.FillFirst<T>(oInstance, GetName(), Species, PrepareParameters());
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

		protected AStoredProcedure(AConnection oDB, ASafeLog oLog = null, CommandSpecies nSpecies = CommandSpecies.StoredProcedure) {
			if (oDB == null)
				throw new ArgumentNullException("oDB", "Database connection not specified.");

			m_aryArgs = null;
			DB = oDB;
			Log = new SafeLog(oLog);
			Species = nSpecies;

			CheckDirection();
		} // constructor

		#endregion constructor

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
						throw new Exception("Multiple 'return value' parameters configured for stored procedure " + GetName());

					bReturnFound = true;
				} // if
			});
		} // CheckDirection

		#endregion method CheckDirection

		#region property DB

		protected virtual AConnection DB { get; private set; } // DB

		#endregion property DB

		#region property Log

		protected virtual ASafeLog Log { get; private set; } // Log

		#endregion property Log

		#region property Species

		protected virtual CommandSpecies Species { get; private set; }

		#endregion property Species

		#region method PrepareParameters

		protected virtual QueryParameter[] PrepareParameters() {
			if (m_aryArgs == null) {
				var args = new List<QueryParameter>();

				this.Traverse((oInstance, oPropertyInfo) => {
					object[] oNameAttrList = oPropertyInfo.GetCustomAttributes(typeof (FieldNameAttribute), false);

					string sFieldName = (oNameAttrList.Length > 0) ? ((FieldNameAttribute)oNameAttrList[0]).Name : oPropertyInfo.Name;

					object[] oDirAttrList = oPropertyInfo.GetCustomAttributes(typeof (DirectionAttribute), false);

					ParameterDirection nDirection = (oDirAttrList.Length > 0) ? ((DirectionAttribute)oDirAttrList[0]).Direction : ParameterDirection.Input;

					QueryParameter qp = null;

					bool bIsSimpleType =
						(oPropertyInfo.PropertyType == typeof (string)) ||
						(null == oPropertyInfo.PropertyType.GetInterface(typeof (IEnumerable).ToString()));

					if (bIsSimpleType) {
						qp = new QueryParameter(sFieldName, oPropertyInfo.GetValue(oInstance, null)) {
							Direction = nDirection,
						};
					}
					else {
						if (null == oPropertyInfo.PropertyType.GetInterface(typeof (ITraversable).ToString()))
							throw new NotImplementedException("Type " + oPropertyInfo.PropertyType + " does not implement " + typeof (ITraversable));

						if (null == oPropertyInfo.PropertyType.GetInterface(typeof (IParametrisable).ToString()))
							throw new NotImplementedException("Type " + oPropertyInfo.PropertyType + " does not implement " + typeof (IParametrisable));

						qp = DB.CreateTableParameter(
							oPropertyInfo.PropertyType,
							sFieldName,
							(IEnumerable)oPropertyInfo.GetValue(oInstance, null),
							o => ((IParametrisable)o).ToParameter(),
							nDirection
							);
					} // if

					args.Add(qp);
				});

				m_aryArgs = args.ToArray();
			} // if

			return m_aryArgs;
		} // PrepareParameters

		#endregion method PrepareParameters

		#region method GetName

		/// <summary>
		/// Returns the name of the stored procedure.
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
} // namespace Ezbob.Database
