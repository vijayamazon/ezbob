namespace Ezbob.Backend.Strategies.CustomerManualAnnualizedRevenue {
	using Ezbob.Database;
	using CmarModel = Ezbob.Backend.Models.CustomerManualAnnualizedRevenue;

	public class GetCustomerManualAnnualizedRevenue : AStrategy {
		public GetCustomerManualAnnualizedRevenue(int nCustomerID) {
			Result = new CmarModel();
			m_nCustomerID = nCustomerID;
		} // constructor

		public override string Name {
			get { return "GetCustomerManualAnnualizedRevenue"; }
		} // Name

		public override void Execute() {
			DB.FillFirst(Result, "GetCustomerManualAnnualizedRevenue", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", m_nCustomerID));
		} // Execute

		public CmarModel Result { get; private set; }

		private readonly int m_nCustomerID;

	} // class GetCustomerManualAnnualizedRevenue
} // namespace
