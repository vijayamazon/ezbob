namespace EzBob.Backend.Strategies.Misc {
	using Ezbob.Database;
	using Ezbob.Logger;
	using Models;

	public class GetConfigTable : AStrategy {
		#region public

		#region constructor

		public GetConfigTable(string sTableName, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_sTableName = sTableName;
			Result = new ConfigTable[0];
		} // constructor

		#endregion constructor

		#region property Result

		public ConfigTable[] Result { get; private set; }

		#endregion property Result

		#region property Name

		public override string Name {
			get { return "GetConfigTable"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Result = DB.Fill<ConfigTable>(
				"GetConfigTable",
				CommandSpecies.StoredProcedure,
				new QueryParameter("TableName", m_sTableName)
			).ToArray();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly string m_sTableName;

		#endregion private
	} // class GetConfigTable
} // namespace
