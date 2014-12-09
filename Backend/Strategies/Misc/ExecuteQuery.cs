namespace EzBob.Backend.Strategies.Misc 
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ExecuteQuery : AStrategy
	{
		private readonly string query;
		public bool IsError { get; private set; }

		public ExecuteQuery(string query, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.query = query;
		} // constructor

		public override string Name {
			get { return "Execute query"; }
		} // Name

		public override void Execute() {
			try
			{
				DB.ExecuteNonQuery(query, CommandSpecies.Text);
			}
			catch (Exception e)
			{
				Log.Error("Failed executing query:{0}. The exception was:{1}", query, e);
				IsError = true;
			}
		} // Execute

	} // class ExecuteQuery
} // namespace
