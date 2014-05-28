namespace EzBob.Backend.Strategies.Misc 
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ExecuteQuery : AStrategy
	{
		private readonly string query;
		public bool IsError { get; private set; }
		#region constructor

		public ExecuteQuery(string query, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.query = query;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Execute query"; }
		} // Name

		#endregion property Name

		#region property Execute

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

		#endregion property Execute
	} // class ExecuteQuery
} // namespace
