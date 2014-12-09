namespace Ezbob.Database {
	using System;
	using Ezbob.Logger;

	public abstract partial class AConnection : SafeLog {

		public virtual void ForEachResult<T>(ConnectionWrapper oConnectionToUse, Action<T> oAction, string sQuery, params QueryParameter[] aryParams) where T : IResultRow, new() {
			ForEachResult<T>(oConnectionToUse, oAction, sQuery, CommandSpecies.Auto, aryParams);
		} // ForEachResult

		public virtual void ForEachResult<T>(Action<T> oAction, string sQuery, params QueryParameter[] aryParams) where T : IResultRow, new() {
			ForEachResult<T>(null, oAction, sQuery, CommandSpecies.Auto, aryParams);
		} // ForEachResult

		public virtual void ForEachResult<T>(Action<T> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) where T : IResultRow, new() {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachResult' call.");

			ForEachResult(null, oAction, sQuery, nSpecies, aryParams);
		} // ForEachResult

		public virtual void ForEachResult<T>(ConnectionWrapper oConnectionToUse, Action<T> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) where T : IResultRow, new() {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachResult' call.");

			Func<T, ActionResult> oFunc = r => {
				oAction(r);
				return ActionResult.Continue;
			};

			ForEachResult(oConnectionToUse, oFunc, sQuery, nSpecies, aryParams);
		} // ForEachResult

		public virtual void ForEachResult<T>(ConnectionWrapper oConnectionToUse, Func<T, ActionResult> oAction, string sQuery, params QueryParameter[] aryParams) where T : IResultRow, new() {
			ForEachResult<T>(oConnectionToUse, oAction, sQuery, CommandSpecies.Auto, aryParams);
		} // ForEachResult

		public virtual void ForEachResult<T>(Func<T, ActionResult> oAction, string sQuery, params QueryParameter[] aryParams) where T : IResultRow, new() {
			ForEachResult<T>(null, oAction, sQuery, CommandSpecies.Auto, aryParams);
		} // ForEachResult

		public virtual void ForEachResult<T>(Func<T, ActionResult> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) where T : IResultRow, new() {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachResult' call.");

			ForEachResult(null, oAction, sQuery, nSpecies, aryParams);
		} // ForEachResult

		public virtual void ForEachResult<T>(ConnectionWrapper oConnectionToUse, Func<T, ActionResult> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) where T : IResultRow, new() {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachResult' call.");

			ForEachRowSafe(
				oConnectionToUse,
				(sr, bRowsetStart) => {
					var oResult = new T();
					oResult.SetIsFirst(bRowsetStart);

					sr.Fill(oResult);

					return oAction(oResult);
				},
				sQuery,
				nSpecies,
				aryParams
			);
		} // ForEachResult

	} // AConnection
} // namespace Ezbob.Database
