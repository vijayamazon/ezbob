namespace Ezbob.Backend.Strategies.Misc {
	using EzBob.Backend.Models;
	using Ezbob.Database;

	public class GetConfigTable : AStrategy {
		public GetConfigTable(string sTableName) {
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
