namespace EzBob.Backend.Strategies.CustomerManualAnnualizedRevenue {
	using Ezbob.Database;
	using Ezbob.Logger;
	using CmarModel = Ezbob.Backend.Models.CustomerManualAnnualizedRevenue;

	public class GetCustomerManualAnnualizedRevenue : AStrategy {
		#region public

		#region constructor

		public GetCustomerManualAnnualizedRevenue(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			Result = new CmarModel();
			m_nCustomerID = nCustomerID;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "GetCustomerManualAnnualizedRevenue"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			DB.FillFirst(Result, "GetCustomerManualAnnualizedRevenue", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", m_nCustomerID));
		} // Execute

		#endregion method Execute

		public CmarModel Result { get; private set; }

		#endregion public

		#region private

		private readonly int m_nCustomerID;

		#endregion private
	} // class GetCustomerManualAnnualizedRevenue
} // namespace
