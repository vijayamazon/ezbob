namespace EzBob.Backend.Strategies 
{
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetSpResultTable : AStrategy
	{
		private readonly string spName;
		private readonly QueryParameter[] parameters;
		public DataTable Result { get; private set; }
		#region constructor

		public GetSpResultTable(AConnection oDb, ASafeLog oLog, string spName, params QueryParameter[] parameters)
			: base(oDb, oLog)
		{
			this.parameters = parameters;
			this.spName = spName;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Get sp result table"; }
		} // Name

		#endregion property Name

		#region property Execute

		public override void Execute() {
			Result = DB.ExecuteReader(spName, CommandSpecies.StoredProcedure, parameters);
		} // Execute

		#endregion property Execute
	} // class GetSpResultTable
} // namespace
