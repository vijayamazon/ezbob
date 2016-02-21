namespace Ezbob.Backend.Strategies.Tasks {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.ManualDecision;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Database;

	public class InvestorFinder : AStrategy {
		public InvestorFinder() {
			this.now = DateTime.UtcNow;
		}//ctor

		public override string Name { get { return "InvestorFinder"; } } // Name

		public override void Execute() {
			Log.Info("Start to search investor for all pending investor");
			DB.ForEachRowSafe(HandleOnePendingInvestor, "UwGridPendingInvestor", CommandSpecies.StoredProcedure, new QueryParameter("WithTest", false));
			Log.Info("End to search investor for all pending investor");
		}// Execute

		private ActionResult HandleOnePendingInvestor(SafeReader sr, bool bRowSetStart) {
			int customerId = sr["CustomerID"];
			long cashRequestID = sr["CashRequestID"];

			Log.Info("Searching investor for customer {0} cash request {1}", customerId, cashRequestID);

			var aiar = new LoadApplicationInfo(
				customerId,
				cashRequestID,
				this.now
			);
			aiar.Execute();

			ApplyManualDecision applyManualDecision = new ApplyManualDecision(new DecisionModel {
				customerID = customerId,
				status = EZBob.DatabaseLib.Model.Database.CreditResultStatus.Approved,
				underwriterID = 1,
				attemptID = Guid.NewGuid().ToString("N"),
				cashRequestID = cashRequestID,
				cashRequestRowVersion = aiar.Result.CashRequestRowVersion,
			});
			applyManualDecision.Execute();
			return ActionResult.Continue;
		}//HandleOnePendingInvestor

		private readonly DateTime now;
	}//class InvestorFinder
} // namespace
