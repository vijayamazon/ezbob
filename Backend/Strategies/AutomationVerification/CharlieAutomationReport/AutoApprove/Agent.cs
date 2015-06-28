namespace Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport.AutoApprove {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.Extensions;
	using Ezbob.Database;
	using Ezbob.Logger;

	using BaseAgent = AutomationCalculator.AutoDecision.AutoApproval.ManAgainstAMachine.SameDataAgent;
	using BaseChecker = AutomationCalculator.AutoDecision.AutoApproval.Checker;
	using CarChecker = Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport.AutoApprove.Checker;

	internal class Agent : BaseAgent {
		public Agent(
			int nCustomerID,
			decimal nSystemCalculatedAmount,
			AutomationCalculator.Common.Medal nMedal,
			AutomationCalculator.Common.MedalType nMedalType,
			AutomationCalculator.Common.TurnoverType? turnoverType,
			DateTime now,
			AConnection oDB,
			ASafeLog oLog
		) : base(nCustomerID, nSystemCalculatedAmount, nMedal, nMedalType, turnoverType, now, oDB, oLog) {
		} // constructor

		public IEnumerable<string> FindBadCaisStatuses() {
			return CaisAccounts
				.Where(ca => CarCaisAccount.IsBad(
					Now,
					ca.LastUpdatedDate,
					Math.Max(ca.Balance ?? 0, ca.CurrentDefBalance ?? 0),
					ca.AccountStatusCodes
				))
				.Select(ca => string.Format(
					"ID {0}, updated on {1}, balance {2}, codes {3}",
					ca.Id,
					(ca.LastUpdatedDate ?? DateTime.UtcNow).DateStr(),
					Math.Max(ca.Balance ?? 0, ca.CurrentDefBalance ?? 0),
					ca.AccountStatusCodes
				));
		} // FindBadCaisStatuses

		protected override BaseChecker CreateChecker() {
			return new CarChecker(this);
		} // CreateChecker

		protected override bool WorstCaisStatusIsRelevant(SafeReader sr) {
			DateTime? lastUpdated = sr["LastUpdatedDate"];
			return lastUpdated.HasValue && (lastUpdated.Value.AddYears(1) >= Now);
		} // WorstCaisStatusIsRelevant
	} // class Agent
} // namespace
