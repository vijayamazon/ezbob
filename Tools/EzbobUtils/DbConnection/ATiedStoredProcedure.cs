namespace Ezbob.Database {
	using System;
	using System.Collections.Generic;
	using System.Data.Common;
	using Ezbob.Logger;

	public abstract class ATiedStoredProcedure : AStoredProcedure {
		public virtual void Execute() {
			if (!IsReadyToGo())
				throw new ArgumentOutOfRangeException("Parameters are invalid for " + GetName(), (Exception)null);

			TiedAction();
		} // Execute

		public override T ExecuteScalar<T>(ConnectionWrapper oConnectionToUse = null) {
			throw CreateTiedException();
		} // ExecuteScalar

		public override int ExecuteNonQuery(ConnectionWrapper oConnectionToUse = null) {
			throw CreateTiedException();
		} // ExecuteNonQuery

		public override void ForEachRow(Func<DbDataReader, bool, ActionResult> oAction) {
			throw CreateTiedException();
		} // ForEachRow

		public override void ForEachRow(ConnectionWrapper oConnectionToUse, Func<DbDataReader, bool, ActionResult> oAction) {
			throw CreateTiedException();
		} // ForEachRow

		public override void ForEachRowSafe(Action<SafeReader> oAction) {
			throw CreateTiedException();
		} // ForEachRowSafe

		public override void ForEachRowSafe(Func<SafeReader, bool, ActionResult> oAction) {
			throw CreateTiedException();
		} // ForEachRowSafe

		public override void ForEachRowSafe(ConnectionWrapper oConnectionToUse, Action<SafeReader> oAction) {
			throw CreateTiedException();
		} // ForEachRowSafe

		public override void ForEachRowSafe(
			ConnectionWrapper oConnectionToUse,
			Func<SafeReader, bool, ActionResult> oAction
		) {
			throw CreateTiedException();
		} // ForEachRowSafe

		public override void ForEachResult(Func<IResultRow, ActionResult> oAction) {
			throw CreateTiedException();
		} // ForEachResult

		public override void ForEachResult(ConnectionWrapper oConnectionToUse, Func<IResultRow, ActionResult> oAction) {
			throw CreateTiedException();
		} // ForEachResult

		public override void ForEachResult<T>(Func<T, ActionResult> oAction) {
			throw CreateTiedException();
		} // ForEachResult

		public override void ForEachResult<T>(ConnectionWrapper oConnectionToUse, Func<T, ActionResult> oAction) {
			throw CreateTiedException();
		} // ForEachResult

		public override List<T> Fill<T>(ConnectionWrapper oConnectionToUse = null) {
			throw CreateTiedException();
		} // Fill

		public override T FillFirst<T>(ConnectionWrapper oConnectionToUse = null) {
			throw CreateTiedException();
		} // FillFirst

		public override void FillFirst<T>(T oInstance) {
			throw CreateTiedException();
		} // FillFirst

		public override void FillFirst<T>(ConnectionWrapper oConnectionToUse, T oInstance) {
			throw CreateTiedException();
		} // FillFirst

		public override IEnumerable<SafeReader> ExecuteEnumerable(ConnectionWrapper oConnectionToUse = null) {
			throw CreateTiedException();
		} // ExecuteEnumerable

		public override SafeReader GetFirst(ConnectionWrapper oConnectionToUse = null) {
			throw CreateTiedException();
		} // GetFirst

		protected ATiedStoredProcedure(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		protected abstract void TiedAction();
	} // class ATiedStoredProcedure

	public abstract class ATiedStoredProc : AStoredProcedure {
		protected ATiedStoredProc(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		/// <summary>
		/// Returns the name of the stored procedure.
		/// Stored procedure name is current class name.
		/// If current class name starts with SP (case insensitive) these two letters are dropped
		/// (i.e. SpDoSomething -> DoSomething).
		/// If current class name is 'SP' (case insensitive) it is used as a stored procedure name.
		/// </summary>
		/// <returns>SP name.</returns>
		protected override string GetName() {
			return CheckForSp(base.GetName());
		} // GetName
	} // class ATiedStoredProc
} // namespace
