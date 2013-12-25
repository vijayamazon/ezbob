namespace EzBob.Backend.Strategies 
{
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class UpdateMarketplaces : AStrategy {
		#region public

		#region constructor

		public UpdateMarketplaces(int customerId, AConnection oDb, ASafeLog oLog) : base(oDb, oLog) {
			this.customerId = customerId;
			m_oDB = oDb;
			m_oLog = oLog;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Update Marketplaces"; }
		} // Name

		#endregion property Name

		#region method UpdateAllMarketplaces

		public override void Execute() {
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
		
		private readonly int customerId;
		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		#endregion private
	} // class UpdateMarketplaces
} // namespace
