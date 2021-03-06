﻿namespace Ezbob.Database {
	using System;
	using System.Data.Common;

	public abstract partial class AConnection {
		public virtual void ForEachRow(ConnectionWrapper oConnectionToUse, Action<DbDataReader> oAction, string sQuery, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			ForEachRow(oConnectionToUse, oAction, sQuery, CommandSpecies.Auto, aryParams);
		} // ForEachRow

		public virtual void ForEachRow(Action<DbDataReader> oAction, string sQuery, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			ForEachRow(null, oAction, sQuery, CommandSpecies.Auto, aryParams);
		} // ForEachRow

		public virtual void ForEachRow(Action<DbDataReader> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			ForEachRow(null, oAction, sQuery, nSpecies, aryParams);
		} // ForEachRow

		public virtual void ForEachRow(ConnectionWrapper oConnectionToUse, Action<DbDataReader> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			Func<DbDataReader, bool, ActionResult> oFunc = (r, bRowsetStart) => {
				oAction(r);
				return ActionResult.Continue;
			};

			ForEachRow(oConnectionToUse, oFunc, sQuery, nSpecies, aryParams);
		} // ForEachRow

		public virtual void ForEachRow(ConnectionWrapper oConnectionToUse, Func<DbDataReader, bool, ActionResult> oAction, string sQuery, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			ForEachRow(oConnectionToUse, oAction, sQuery, CommandSpecies.Auto, aryParams);
		} // ForEachRow

		public virtual void ForEachRow(Func<DbDataReader, bool, ActionResult> oAction, string sQuery, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			ForEachRow(null, oAction, sQuery, CommandSpecies.Auto, aryParams);
		} // ForEachRow

		public virtual void ForEachRow(Func<DbDataReader, bool, ActionResult> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			ForEachRow(null, oAction, sQuery, nSpecies, aryParams);
		} // ForEachRow

		public virtual void ForEachRow(ConnectionWrapper oConnectionToUse, Func<DbDataReader, bool, ActionResult> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			Run(oConnectionToUse, oAction, ExecMode.ForEachRow, nSpecies, sQuery, aryParams);
		} // ForEachRow
	} // class AConnection
} // namespace Ezbob.Database
