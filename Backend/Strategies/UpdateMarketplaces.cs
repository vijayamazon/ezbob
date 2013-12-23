namespace EzBob.Backend.Strategies 
{
	using System.Data;
	using EZBob.DatabaseLib;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StructureMap;

	public class UpdateMarketplaces : AStrategy {
		#region public

		#region constructor

		public UpdateMarketplaces(int customerId, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			this.customerId = customerId;
			m_oDB = oDB;
			m_oLog = oLog;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Update Marketplaces"; }
		} // Name

		#endregion property Name

		#region method UpdateAllMarketplaces

		public override void Execute()
		{
			DataTable dt = DB.ExecuteReader(
				"GetCustomerMarketplaces",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			foreach (DataRow row in dt.Rows) {
				int marketplaceId = int.Parse(row["Id"].ToString());
				new UpdateMarketplace(customerId, marketplaceId, m_oDB, m_oLog).Execute();
			} // foreach
		} // Execute

		#endregion method UpdateAllMarketplaces

		#endregion public

		#region private

		#region property Helper

		private DatabaseDataHelper Helper {
			get { return ObjectFactory.GetInstance<DatabaseDataHelper>(); }
		} // Helper

		#endregion property Helper

		private readonly int customerId;
		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		#endregion private
	} // class UpdateMarketplaces
} // namespace
