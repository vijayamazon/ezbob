namespace Ezbob.Database {
	using System;
	using System.Data.Common;

	#region class DbCommandExt

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
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			Run(
				cmd,
				(oReader, bRowSetStart) => oAction(new SafeReader(oReader), bRowSetStart),
				oLogExecution
			);
		} // ForEachRowSafe

		#endregion method ForEachRowSafe

		#endregion public

		#region private

		private static void Run(DbCommand command, Func<DbDataReader, bool, ActionResult> oAction, Action oLogExecution) {
			var oReader = command.ExecuteReader();

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
		} // Run

		#endregion private
	} // class DbCommandExt

	#endregion class DbCommandExt
} // namespace Ezbob.Database
