namespace EzBob.Backend.Strategies.Misc {
	using Ezbob.Database;
	using Ezbob.Logger;
	using Models;

	public class GetConfigTable : AStrategy {

		public GetConfigTable(string sTableName, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_sTableName = sTableName;
			Result = new ConfigTable[0];
		} // constructor

		public ConfigTable[] Result { get; private set; }

		public override string Name {
			get { return "GetConfigTable"; }
		} // Name

		public override void Execute() {
			Result = DB.Fill<ConfigTable>(
				"GetConfigTable",
				CommandSpecies.StoredProcedure,
				new QueryParameter("TableName", m_sTableName)
			).ToArray();
		} // Execute

		private readonly string m_sTableName;

	} // class GetConfigTable
} // namespace
