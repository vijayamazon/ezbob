namespace Ezbob.Backend.Strategies.Misc {
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	
	public class LoadDecisionHistory : AStrategy {
		public LoadDecisionHistory(int customerID) {
			this.customerID = customerID;
		} // constructor

		public override string Name {
			get { return "LoadDecisionHistory"; }
		} // Name

		public override void Execute() {
			Result = DB.Fill<DecisionHistoryDBModel>("LoadDecisionHistory", CommandSpecies.StoredProcedure, new QueryParameter("@CustomerID", this.customerID));
		}// Execute

		public IEnumerable<DecisionHistoryDBModel> Result { get; private set; }
		private readonly int customerID;
	} // class LoadDecisionHistory
} // namespace

