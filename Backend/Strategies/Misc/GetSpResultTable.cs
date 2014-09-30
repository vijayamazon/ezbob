namespace EzBob.Backend.Strategies.Misc 
{
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetSpResultTable : AStrategy
	{
		private readonly string spName;
		private readonly string[] parameters;
		public DataTable Result { get; private set; }

		#region constructor

		public GetSpResultTable(AConnection oDb, ASafeLog oLog, string spName, params string[] parameters)
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
			var sqlParams = new List<QueryParameter>();
			int counter = 1;
			if (parameters != null)
			{
				while (counter < parameters.Length)
				{
					sqlParams.Add(new QueryParameter(parameters[counter - 1], parameters[counter]));
					counter += 2;
				}
			}

			Result = DB.ExecuteReader(spName, CommandSpecies.StoredProcedure, sqlParams.ToArray());
		} // Execute

		#endregion property Execute
	} // class GetSpResultTable
} // namespace
