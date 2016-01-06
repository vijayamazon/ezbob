namespace Ezbob.Database {
	public class TiedException : DbException {
		public TiedException(string spName) : base(
			string.Format(
				"Stored procedure {0} is tied (standard actions are prohibited; " +
				"override TiedAction() method and call Execute()).",
				spName
			)
		) { } // constructor
	} // class TiedException
} // namespace
