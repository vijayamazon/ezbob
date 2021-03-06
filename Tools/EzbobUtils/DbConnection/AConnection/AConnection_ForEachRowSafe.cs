﻿namespace Ezbob.Database {
	using System;

	public abstract partial class AConnection {
		public virtual void ForEachRowSafe(ConnectionWrapper oConnectionToUse, Action<SafeReader> oAction, string sQuery, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			ForEachRowSafe(oConnectionToUse, oAction, sQuery, CommandSpecies.Auto, aryParams);
		} // ForEachRowSafe

		public virtual void ForEachRowSafe(Action<SafeReader> oAction, string sQuery, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			ForEachRowSafe(null, oAction, sQuery, CommandSpecies.Auto, aryParams);
		} // ForEachRowSafe

		public virtual void ForEachRowSafe(Action<SafeReader> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			ForEachRowSafe(null, oAction, sQuery, nSpecies, aryParams);
		} // ForEachRowSafe

		public virtual void ForEachRowSafe(ConnectionWrapper oConnectionToUse, Action<SafeReader> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			Func<SafeReader, bool, ActionResult> oFunc = (sr, bRowsetStart) => {
				oAction(sr);
				return ActionResult.Continue;
			};

			ForEachRowSafe(oConnectionToUse, oFunc, sQuery, nSpecies, aryParams);
		} // ForEachRowSafe

		public virtual void ForEachRowSafe(ConnectionWrapper oConnectionToUse, Func<SafeReader, bool, ActionResult> oAction, string sQuery, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			ForEachRowSafe(oConnectionToUse, oAction, sQuery, CommandSpecies.Auto, aryParams);
		} // ForEachRowSafe

		public virtual void ForEachRowSafe(Func<SafeReader, bool, ActionResult> oAction, string sQuery, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			ForEachRowSafe(null, oAction, sQuery, CommandSpecies.Auto, aryParams);
		} // ForEachRowSafe

		public virtual void ForEachRowSafe(Func<SafeReader, bool, ActionResult> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			ForEachRowSafe(null, oAction, sQuery, nSpecies, aryParams);
		} // ForEachRowSafe

		public virtual void ForEachRowSafe(ConnectionWrapper oConnectionToUse, Func<SafeReader, bool, ActionResult> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			Run(
				oConnectionToUse,
				(oReader, bRowSetStart) => oAction(new SafeReader(oReader), bRowSetStart),
				ExecMode.ForEachRow, nSpecies, sQuery, aryParams
			);
		} // ForEachRowSafe
	} // class AConnection
} // namespace Ezbob.Database
