namespace Ezbob.Database {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.Common;
	using Ezbob.Utils;

	public interface IStoredProcedure {
		T ExecuteScalar<T>();
		DataTable ExecuteReader();
		int ExecuteNonQuery();
		void ForEachRow(Func<DbDataReader, bool, ActionResult> oAction);
		void ForEachRowSafe(Func<SafeReader, bool, ActionResult> oAction);
		void ForEachResult(Func<IResultRow, ActionResult> oAction);
		void ForEachResult<T>(Func<T, ActionResult> oAction) where T: IResultRow, new();
		List<T> Fill<T>() where T: ITraversable, new();
		T FillFirst<T>() where T: ITraversable, new();
	} // interface IStoredProcedure
} // namespace Ezbob.Database
