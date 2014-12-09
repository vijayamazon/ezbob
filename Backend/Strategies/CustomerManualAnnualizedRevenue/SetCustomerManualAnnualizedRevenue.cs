namespace EzBob.Backend.Strategies.CustomerManualAnnualizedRevenue {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using CmarModel = Ezbob.Backend.Models.CustomerManualAnnualizedRevenue;

	public class SetCustomerManualAnnualizedRevenue : AStrategy {

		public SetCustomerManualAnnualizedRevenue(int nCustomerID, decimal nRevenue, string sComment, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			Result = new CmarModel {
				Comment = sComment,
				EntryTime = DateTime.UtcNow,
			};

			m_nCustomerID = nCustomerID;
			m_nRevenue = nRevenue;
		} // constructor

		public override string Name {
			get { return "GetCustomerManualAnnualizedRevenue"; }
		} // Name

		public override void Execute() {
			DB.ExecuteNonQuery(
				"SetCustomerManualAnnualizedRevenue",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", m_nCustomerID),
				new QueryParameter("Revenue", m_nRevenue),
				new QueryParameter("Comment", Result.Comment),
				new QueryParameter("Now", Result.EntryTime)
			);

			Result.Revenue = m_nRevenue;
		} // Execute

		public CmarModel Result { get; private set; }

		private readonly int m_nCustomerID;
		private readonly decimal m_nRevenue;

	} // class SetCustomerManualAnnualizedRevenue
} // namespace
