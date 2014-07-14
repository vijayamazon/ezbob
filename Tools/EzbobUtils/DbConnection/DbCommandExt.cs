namespace Ezbob.Database {
	using System;
	using System.Collections.Generic;
	using System.Data.Common;

	public static class DbCommandExt {
		#region public

		#region method ForEachRow

		public static void ForEachRow(this DbCommand cmd, Func<DbDataReader, bool, ActionResult> oAction, Action oLogExecution = null) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			Run(cmd, oAction, oLogExecution);
		} // ForEachRow

		#endregion method ForEachRow

		#region method ForEachRowSafe

		public static void ForEachRowSafe(this DbCommand cmd, Func<SafeReader, bool, ActionResult> oAction, Action oLogExecution = null) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRowSafe' call.");

			Run(
				cmd,
				(oReader, bRowSetStart) => oAction(new SafeReader(oReader), bRowSetStart),
				oLogExecution
			);
		} // ForEachRowSafe

		#endregion method ForEachRowSafe

		#region method ForEachResult

		public static void ForEachResult<T>(this DbCommand cmd, Func<T, ActionResult> oAction, Action oLogExecution = null) where T: IResultRow, new() {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachResult' call.");

			Run(
				cmd,
				(oReader, bRowSetStart) => {
					var sr = new SafeReader(oReader);
					T oResult = sr.Fill<T>();
					oResult.SetIsFirst(bRowSetStart);
					return oAction(oResult);
				},
				oLogExecution
			);
		} // ForEachResult

		#endregion method ForEachResult

		#region method ExecuteEnumerable

		public static IEnumerable<SafeReader> ExecuteEnumerable(this DbCommand command, ConnectionWrapper cw, bool bForceCloseOnExit, Action oLogExecution = null) {
			cw.Open();

			DbDataReader oReader = command.ExecuteReader();

			if (oLogExecution != null)
				oLogExecution();

			do {
				if (!oReader.HasRows)
					continue;

				while (oReader.Read())
					yield return new SafeReader(oReader);
			} while (oReader.NextResult());

			oReader.Close();

			if (bForceCloseOnExit)
				cw.Close();
		} // ExecuteEnumerable

		#endregion method ExecuteEnumerable

		#endregion public

		#region private

		private static void Run(DbCommand command, Func<DbDataReader, bool, ActionResult> oAction, Action oLogExecution) {
			DbDataReader oReader = command.ExecuteReader();

			if (oLogExecution != null)
				oLogExecution();

			bool bStop = false;

			do {
				if (!oReader.HasRows)
					continue;

				bool bRowSetStart = true;

				while (oReader.Read()) {
					ActionResult nResult = oAction(oReader, bRowSetStart);

					if (nResult == ActionResult.SkipCurrent)
						break;

					if (nResult == ActionResult.SkipAll) {
						bStop = true;
						break;
					} // if

					bRowSetStart = false;
				} // while has rows in current set
			} while (!bStop && oReader.NextResult());

			oReader.Close();
		} // Run

		#endregion private
	} // class DbCommandExt
} // namespace Ezbob.Database
