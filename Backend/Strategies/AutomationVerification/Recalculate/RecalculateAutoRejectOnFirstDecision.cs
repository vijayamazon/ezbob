﻿namespace Ezbob.Backend.Strategies.AutomationVerification.Recalculate {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Backend.Extensions;
	using Ezbob.Database;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.ManAgainstAMachine;
	using Ezbob.Utils;

	public class RecalculateAutoRejectOnFirstDecision : AStrategy {
		public override string Name {
			get { return "RecalculateAutoRejectOnFirstDecision"; }
		} // Name

		public override void Execute() {
			this.output = new List<string>();

				this.output.Add(string.Format("header|{0}", string.Join("|",
					"UniqueID",
					"DecisionStatus",
					"DecisionTime",
					"customerID",
					"cashRequestID"
				)));

			this.tag = string.Format(
				"#{0}_{1}",
				Name,
				DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture)
			);

			DB.ForEachRowSafe(ProcessRow, LoadCustomersQuery, CommandSpecies.Text);

			Log.Debug("Output data:\n\n{0}\n\n", string.Join("\n", this.output));
		} // Execute

		private void ProcessRow(SafeReader sr) {
			int customerID = sr["CustomerID"];
			long cashRequestID = sr["CashRequestID"];
			DateTime decisionTime = sr["DecisionTime"];

			try {
				var agent = new SameDataAgent(customerID, cashRequestID, decisionTime, DB, Log);
				agent.Decide(this.tag);

				this.output.Add(string.Format("decision|{0}", string.Join("|",
					agent.Trail.UniqueID,
					agent.Trail.DecisionStatus,
					decisionTime.MomentStr(),
					customerID,
					cashRequestID
				)));

				agent.Trail.InputData.TraverseReadable((instance, pi) => {
					this.output.Add(string.Format("parameter|{0}", string.Join("|",
						agent.Trail.UniqueID,
						pi.Name,
						pi.GetValue(instance)
					)));
				});
			} catch (Exception e) {
				Log.Warn(
					e,
					"Failed to recalculate auto reject for customer {0} with cash request {1} decided at {2}.",
					customerID,
					cashRequestID,
					decisionTime.MomentStr()
				);
			} // try
		} // ProcessRow

		private string tag;
		private List<string> output;

		private const string LoadCustomersQuery = @";WITH cd AS (
	SELECT
		CustomerID = c.Id,
		CashRequestID = r.Id,
		DecisionTime = r.UnderwriterDecisionDate,
		Position = ROW_NUMBER() OVER (PARTITION BY r.IdCustomer ORDER BY r.UnderwriterDecisionDate)
	FROM
		Customer c
		INNER JOIN CashRequests r ON c.Id = r.IdCustomer AND r.UnderwriterDecisionDate IS NOT NULL
	WHERE
		c.IsTest = 0
) SELECT
	CustomerID,
	CashRequestID,
	DecisionTime
FROM
	cd
WHERE
	Position = 1
ORDER BY
	CustomerID DESC";
	} // class RecalculateAutoRejectOnFirstDecision
} // namespace

