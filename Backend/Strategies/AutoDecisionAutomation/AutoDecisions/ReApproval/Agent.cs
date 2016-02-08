namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReApproval {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.ReApproval;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;

	public class Agent : AAutoDecisionBase {
		public virtual ReapprovalTrail Trail { get; private set; }

		public virtual int ApprovedAmount { get; private set; }

		public Agent(
			int nCustomerID,
			long? cashRequestID,
			long? nlCashRequestID,
			string tag,
			AConnection oDB,
			ASafeLog oLog
		) {
			DB = oDB;
			Log = oLog.Safe();
			Args = new Arguments(nCustomerID, cashRequestID, nlCashRequestID);
			this.tag = tag;
		} // constructor

		public virtual Agent Init() {
			Now = DateTime.UtcNow;

			MetaData = new MetaData();
			LatePayments = new List<Payment>();
			NewMarketplaces = new List<Marketplace>();
			ApprovedAmount = 0;

			Trail = new ReapprovalTrail(
				Args.CustomerID,
				Args.CashRequestID,
				Args.NLCashRequestID,
				Log,
				CurrentValues.Instance.AutomationExplanationMailReciever,
				CurrentValues.Instance.MailSenderEmail,
				CurrentValues.Instance.MailSenderName
			);

			Trail.SetTag(this.tag);

			Funds = new AvailableFunds();

			Cfg = InitCfg();

			return this;
		} // Init

		public override void MakeAndVerifyDecision() {
			try {
				RunPrimary();

				AutomationCalculator.AutoDecision.AutoReApproval.Agent oSecondary = RunSecondary();

				WasMismatch = !Trail.EqualsTo(oSecondary.Trail);

				Log.Debug("Auto re-approval matching result: {0}match.", WasMismatch ? "mis" : string.Empty);

				if (!WasMismatch && Trail.HasDecided) {
					Log.Debug("Match and approved.");

					if (ApprovedAmount == oSecondary.Result.ReApproveAmount) {
						Log.Debug("Auto re-approval: match and approved and same amount of {0}.", ApprovedAmount);

						Trail.Affirmative<SameAmount>(false).Init(ApprovedAmount);
						oSecondary.Trail.Affirmative<SameAmount>(false).Init(oSecondary.Result.ReApproveAmount);
					} else {
						Log.Debug(
							"Auto re-approval: match and approved but different amounts: {0} primary vs {1} secondary.",
							ApprovedAmount,
							oSecondary.Result.ReApproveAmount
						);

						Trail.Negative<SameAmount>(false).Init(ApprovedAmount);
						oSecondary.Trail.Negative<SameAmount>(false).Init(oSecondary.Result.ReApproveAmount);

						WasMismatch = true;
					} // if
				} // if

				Output.ApprovedAmount = ApprovedAmount;

				Trail.Save(DB, oSecondary.Trail);
			} catch (Exception e) {
				Log.Error(e, "Exception during re-approval.");
				StepFailed<ExceptionThrown>().Init(e);
				Output.ApprovedAmount = 0;
			} // try
		} // MakeAndVerifyDecision

		public override bool WasException {
			get { return Trail.FindTrace<ExceptionThrown>() != null; }
		} // WasException

		public override bool AffirmativeDecisionMade {
			get { return Trail.HasDecided; }
		} // AffirmativeDecisionMade

		public virtual DateTime Now { get; protected set; }

		public AutoReapprovalOutput Output { get; private set; }

		protected virtual Configuration InitCfg() {
			return new Configuration(DB, Log);
		} // InitCfg

		protected virtual AConnection DB { get; private set; }

		protected virtual ASafeLog Log { get; private set; }

		protected virtual Configuration Cfg { get; private set; }

		protected virtual Arguments Args { get; private set; }

		protected virtual MetaData MetaData { get; private set; }

		protected virtual List<Payment> LatePayments { get; private set; }

		protected virtual List<Marketplace> NewMarketplaces { get; private set; }

		protected virtual void GatherData() {
			Cfg.Load();

			DB.ForEachRowSafe(
				ProcessRow,
				"LoadAutoReapprovalData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", Args.CustomerID),
				new QueryParameter("Now", Now)
			);

			GatherAvailableFunds();

			MetaData.Validate();

			Trail.MyInputData.Init(Now, null);
			Trail.MyInputData.ReApproveAmount = MetaData.ApprovedAmount;
			Trail.MyInputData.FraudStatus = MetaData.FraudStatus;
			Trail.MyInputData.ManualApproveDate = MetaData.LacrTime;
			Trail.MyInputData.WasLate = MetaData.LateLoanCount > 0;
			Trail.MyInputData.WasRejected = MetaData.RejectAfterLacrID > 0;
			Trail.MyInputData.NumOutstandingLoans = MetaData.OpenLoanCount;
			Trail.MyInputData.HasLoanCharges = MetaData.SumOfCharges > 0.00000001m;
			Trail.MyInputData.LacrID = MetaData.LacrID;

			Trail.MyInputData.MaxLateDays = LatePayments.Count < 1 ? 0 : LatePayments.Select(lp => lp.Delay).Max();
			Trail.MyInputData.NewDataSourceAdded = NewMarketplaces.Count > 0;
			Trail.MyInputData.AvaliableFunds = Funds.Available - Funds.Reserved;
			Trail.MyInputData.AutoReApproveMaxLacrAge = Cfg.MaxLacrAge;
			Trail.MyInputData.AutoReApproveMaxLatePayment = Cfg.MaxLatePayment;
			Trail.MyInputData.AutoReApproveMaxNumOfOutstandingLoans = Cfg.MaxNumOfOutstandingLoans;
			Trail.MyInputData.MinLoan = ConfigManager.CurrentValues.Instance.MinLoan;

			Output = new AutoReapprovalOutput {
				AppValidFor = Now.AddHours(MetaData.OfferLength),
				IsEmailSendingBanned = MetaData.IsEmailSendingBanned,
				LastApprovedCashRequestID = MetaData.LacrID,
			};
		} // GatherData

		protected virtual AvailableFunds Funds { get; set; }

		protected virtual void GatherAvailableFunds() {
			DB.GetFirst("GetAvailableFunds", CommandSpecies.StoredProcedure).Fill(Funds);
		} // GatherAvailableFunds

		protected virtual void RunPrimary() {
			Log.Debug("Primary: checking if auto re-approve should take place for customer {0}...", Args.CustomerID);

			GatherData();

			CheckInit();
			CheckIsFraud();
			CheckLacrTooOld();
			CheckRejectAfterLacr();
			CheckLateLoans();
			CheckLatePayments();
			CheckNewMarketplaces();
			CheckOutstandingLoans();
			CheckLoanCharges();
			SetApprovedAmount();
			CheckAvailableFunds();
			CheckComplete();

			Log.Debug(
				"Primary: checking if auto re-approve should take place for customer {0} complete.\n{1}",
				Args.CustomerID,
				Trail
			);
		} // RunPrimary

		private enum RowType {
			MetaData,
			LatePayment,
			Marketplace,
		} // enum RowType

		private AutomationCalculator.AutoDecision.AutoReApproval.Agent RunSecondary() {
			var oSecondary = new AutomationCalculator.AutoDecision.AutoReApproval.Agent(
				DB,
				Log,
				Args.CustomerID,
				Args.CashRequestID,
				Args.NLCashRequestID,
				Trail.InputData.DataAsOf
			);
			oSecondary.MakeDecision(oSecondary.GetInputData());

			return oSecondary;
		} // RunSecondary

		private void CheckComplete() {
			if (ApprovedAmount >= Trail.MyInputData.MinLoan)
				StepDone<Complete>().Init(ApprovedAmount, Trail.MyInputData.MinLoan, units: "£");
			else
				StepFailed<Complete>().Init(ApprovedAmount, Trail.MyInputData.MinLoan, units: "£");
		} // CheckComplete

		private void CheckInit() {
			if (MetaData.ValidationErrors.Count == 0)
				StepDone<InitialAssignment>().Init(MetaData.ValidationErrors);
			else
				StepFailed<InitialAssignment>().Init(MetaData.ValidationErrors);
		} // CheckInit

		private void CheckIsFraud() {
			if (MetaData.FraudStatus == FraudStatus.Ok)
				StepDone<FraudSuspect>().Init(MetaData.FraudStatus);
			else
				StepFailed<FraudSuspect>().Init(MetaData.FraudStatus);
		} // CheckIsFraud

		private void CheckLacrTooOld() {
			if (MetaData.LacrIsTooOld(Cfg.MaxLacrAge))
				StepFailed<LacrTooOld>().Init(MetaData.LacrAge ?? -1, Cfg.MaxLacrAge);
			else
				StepDone<LacrTooOld>().Init(MetaData.LacrAge ?? -1, Cfg.MaxLacrAge);
		} // CheckLacrTooOld

		private void CheckRejectAfterLacr() {
			if (MetaData.RejectAfterLacrID > 0)
				StepFailed<RejectAfterLacr>().Init(MetaData.RejectAfterLacrID, MetaData.LacrID);
			else
				StepDone<RejectAfterLacr>().Init(MetaData.RejectAfterLacrID, MetaData.LacrID);
		} // CheckRejectAfterLacr

		private void CheckLateLoans() {
			if (MetaData.LateLoanCount > 0)
				StepFailed<LateLoans>().Init();
			else
				StepDone<LateLoans>().Init();
		} // CheckLateLoans

		private void CheckLatePayments() {
			if (Trail.MyInputData.MaxLateDays <= Trail.MyInputData.AutoReApproveMaxLatePayment)
				StepDone<LatePayment>().Init(Trail.MyInputData.MaxLateDays, Trail.MyInputData.AutoReApproveMaxLatePayment);
			else
				StepFailed<LatePayment>().Init(Trail.MyInputData.MaxLateDays, Trail.MyInputData.AutoReApproveMaxLatePayment);

			/*
			bool bHasLatePayments = false;

			foreach (Payment lp in LatePayments) {
				if (lp.IsLate(Cfg.MaxLatePayment)) {
					bHasLatePayments = true;

					lp.Fill(StepFailed<LatePayment>(), Cfg.MaxLatePayment);
				} // if
			} // for each

			if (!bHasLatePayments)
				StepDone<LatePayment>().Init(0, 0, Now, 0, Now, Cfg.MaxLatePayment);
			*/
		} // CheckLatePayments

		private void CheckNewMarketplaces() {
			if (NewMarketplaces.Count < 1)
				StepDone<NewMarketplace>().Init(null);
			else {
				StepFailed<NewMarketplace>().Init(NewMarketplaces.Select(x => string.Format("a marketplace {1}({2} - {3}) on {0}",
					x.AddTime, x.Name, x.ID, x.Type)).Aggregate((a, b) => a + ", " + b));
			} // if
		} // CheckNewMarketplaces

		private void CheckOutstandingLoans() {
			if (MetaData.OpenLoanCount > Cfg.MaxNumOfOutstandingLoans)
				StepFailed<OutstandingLoanCount>().Init(MetaData.OpenLoanCount, Cfg.MaxNumOfOutstandingLoans);
			else
				StepDone<OutstandingLoanCount>().Init(MetaData.OpenLoanCount, Cfg.MaxNumOfOutstandingLoans);
		} // CheckOutstandingLoans

		private void CheckLoanCharges() {
			if (MetaData.SumOfCharges > 0)
				StepFailed<Charges>().Init(MetaData.SumOfCharges);
			else
				StepDone<Charges>().Init(0);
		} // CheckLoanCharges

		private void SetApprovedAmount() {
			if (Trail.HasDecided)
				ApprovedAmount = (int)Math.Truncate(MetaData.ApprovedAmount);

			if (ApprovedAmount > 0)
				StepDone<ApprovedAmount>().Init(ApprovedAmount);
			else
				StepFailed<ApprovedAmount>().Init(ApprovedAmount);
		} // SetApprovedAmount

		private void CheckAvailableFunds() {
			if (ApprovedAmount < Trail.MyInputData.AvaliableFunds)
				StepDone<EnoughFunds>().Init(ApprovedAmount, Trail.MyInputData.AvaliableFunds);
			else
				StepFailed<EnoughFunds>().Init(ApprovedAmount, Trail.MyInputData.AvaliableFunds);
		} // CheckAvailableFunds

		private void ProcessRow(SafeReader sr) {
			RowType nRowType;

			string sRowType = sr["RowType"];

			if (!Enum.TryParse(sRowType, out nRowType)) {
				Log.Alert("Unsupported row type encountered: '{0}'.", sRowType);
				return;
			} // if

			switch (nRowType) {
			case RowType.MetaData:
				sr.Fill(MetaData);
				break;

			case RowType.LatePayment:
				LatePayments.Add(sr.Fill<Payment>());
				break;

			case RowType.Marketplace:
				NewMarketplaces.Add(sr.Fill<Marketplace>());
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ProcessRow

		/// <summary>
		///     Sets overall decision to 'no approve'.
		/// </summary>
		/// <typeparam name="T">Step type.</typeparam>
		/// <returns>Step type instance for filling step details.</returns>
		private T StepFailed<T>() where T : ATrace {
			ApprovedAmount = 0;
			return Trail.Negative<T>(true);
		} // StepFailed

		/// <summary>
		///     If the step was the only step then overall decision would be 'approved'.
		/// </summary>
		/// <typeparam name="T">Step type.</typeparam>
		/// <returns>Step type instance for filling step details.</returns>
		private T StepDone<T>() where T : ATrace {
			return Trail.Affirmative<T>(false);
		} // StepFailed

		private readonly string tag;
	} // class Agent
} // namespace
