namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class AutoDecisionMaker
	{
		private readonly AConnection db;
		private readonly ASafeLog log;

		public AutoDecisionMaker(AConnection db, ASafeLog log)
		{
			this.db = db;
			this.log = log;
		}

		public void LogDecision(int customerId, AutoDecisionRejectionResponse autoDecisionRejectionResponse, AutoDecisionResponse autoDecisionResponse)
		{
			string decisionName = autoDecisionRejectionResponse.DecidedToReject ? autoDecisionRejectionResponse.DecisionName : autoDecisionResponse.DecisionName;

			int decisionId = db.ExecuteScalar<int>(
				"AutoDecisionRecord",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("DecisionName", decisionName),
				new QueryParameter("Date", DateTime.UtcNow)
			);

			foreach (AutoDecisionCondition condition in autoDecisionRejectionResponse.RejectionConditions)
			{
				db.ExecuteNonQuery(
					"AutoDecisionConditionRecord",
					CommandSpecies.StoredProcedure,
					new QueryParameter("DecisionId", decisionId),
					new QueryParameter("DecisionName", condition.DecisionName),
					new QueryParameter("Satisfied", condition.Satisfied),
					new QueryParameter("Description", condition.Description)
				);
			}
		}
	}
}
