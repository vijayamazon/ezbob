namespace Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport.AutoApprove {
	using System;
	using System.Collections.Generic;
	using System.Linq;
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
			this.badCaisAccounts = new List<CaisAccount>();
		} // constructor

		public IEnumerable<string> FindBadCaisStatuses() {
			return this.badCaisAccounts.Select(ca => ca.ToString());
		} // FindBadCaisStatuses

		protected override BaseChecker CreateChecker() {
			return new CarChecker(this);
		} // CreateChecker

		protected override void GatherData() {
			base.GatherData();

			Trail.MyInputData.WorstStatusList.Clear();

			Trail.MyInputData.SetWorstStatuses(FindBadCaisStatuses());
		} // GatherData

		protected override void ProcessRow(SafeReader sr) {
			base.ProcessRow(sr);

			if (LastRowType != RowType.Cais)
				return;

			CaisAccount ca = sr.Fill<CaisAccount>();

			if (ca.IsBad(Now))
				this.badCaisAccounts.Add(ca);
		} // ProcessRow

		private readonly List<CaisAccount> badCaisAccounts;
	} // class Agent
} // namespace
