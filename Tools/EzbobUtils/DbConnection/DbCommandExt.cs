namespace Ezbob.Database {
	using System;
	using System.Collections.Generic;
	using System.Data.Common;

	public static class DbCommandExt {

		public static void ForEachRow(this DbCommand cmd, Action<DbDataReader> oAction, Action oLogExecution = null) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			Func<DbDataReader, bool, ActionResult> oFunc = (r, bRowsetStart) => {
				oAction(r);
				return ActionResult.Continue;
			};

			cmd.ForEachRow(oFunc, oLogExecution);
		} // ForEachRow

		public static void ForEachRow(this DbCommand cmd, Func<DbDataReader, bool, ActionResult> oAction, Action oLogExecution = null) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			Run(cmd, oAction, oLogExecution);
		} // ForEachRow

		public static void ForEachRowSafe(this DbCommand cmd, Action<SafeReader> oAction, Action oLogExecution = null) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRowSafe' call.");

			Func<SafeReader, bool, ActionResult> oFunc = (sr, bRowsetStart) => {
				oAction(sr);
				return Database.ActionResult.Continue;
			};

			cmd.ForEachRowSafe(oFunc, oLogExecution);
		} // ForEachRowSafe

		public static void ForEachRowSafe(this DbCommand cmd, Func<SafeReader, bool, ActionResult> oAction, Action oLogExecution = null) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRowSafe' call.");

			Run(
				cmd,
				(oReader, bRowSetStart) => oAction(new SafeReader(oReader), bRowSetStart),
				oLogExecution
			);
		} // ForEachRowSafe

		public static void ForEachResult<T>(this DbCommand cmd, Action<T> oAction, Action oLogExecution = null) where T : IResultRow, new() {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachResult' call.");

			Func<T, ActionResult> oFunc = r => {
				oAction(r);
				return ActionResult.Continue;
			};

			cmd.ForEachResult(oFunc, oLogExecution);
		} // ForEachResult

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

		public static IEnumerable<SafeReader> ExecuteEnumerable(
			this DbCommand command,
			ConnectionWrapper cw,
			AConnection oConnection,
			Action oLogExecution = null
		) {
			bool bAllesInOrdnung = false;

			try {
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

				bAllesInOrdnung = true;
			}
			finally {
				if (oConnection != null)
					oConnection.DisposeAfterOneUsage(bAllesInOrdnung, cw);
			} // try
		} // ExecuteEnumerable

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

	} // class DbCommandExt
} // namespace Ezbob.Database
