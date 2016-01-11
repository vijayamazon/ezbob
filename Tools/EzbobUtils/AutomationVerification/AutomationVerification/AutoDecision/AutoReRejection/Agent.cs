namespace AutomationCalculator.AutoDecision.AutoReRejection {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoReRejection;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class Agent {
		public Agent(
			int nCustomerID,
			long? cashRequestID,
			long? nlCashRequestID,
			DateTime? dataAsOf,
			AConnection oDB,
			ASafeLog oLog
		) {
			Log = oLog.Safe();
			DB = oDB;

			Args = new Arguments(nCustomerID, cashRequestID, nlCashRequestID, dataAsOf);
		} // constructor

		public virtual Agent Init() {
			Trail = new ReRejectionTrail(Args.CustomerID, Args.CashRequestID, Args.NlCashRequestID, Log);

			Cfg = new Configuration(DB, Log);

			NewMarketplaces = new List<Marketplace>();
			MetaData = new MetaData();

			return this;
		} // Init

		public virtual void MakeDecision() {
			Log.Debug("Secondary: checking if auto re-reject should take place for customer {0}...", Args.CustomerID);

			try {
				GatherData();

				CheckNumOfLoans();
				CheckReturnRatio();
				CheckLastDecisionWasReject();
				CheckNewMarketplaces();
				CheckRejectionAge();
			} catch (Exception e) {
				Log.Error(e, "Exception during auto re-rejection.");
				StepNoReject<ExceptionThrown>().Init(e);
			} // try

			Log.Debug(
				"Secondary: checking if auto re-reject should take place for customer {0} complete; {1}",
				Args.CustomerID,
				Trail
			);
		} // MakeDecision

		public virtual ReRejectionTrail Trail { get; private set; }

		protected virtual Arguments Args { get; private set; }
		protected virtual Configuration Cfg { get; private set; }
		protected virtual MetaData MetaData { get; private set; }
		protected virtual List<Marketplace> NewMarketplaces { get; private set; }

		protected virtual DateTime Now {
			get { return Trail.InputData.DataAsOf; }
		} // Now

		protected virtual AConnection DB { get; private set; }
		protected virtual ASafeLog Log { get; private set; }

		protected virtual void GatherData() {
			Cfg.Load();

			DB.ForEachRowSafe(
				ProcessRow,
				"LoadAutoRerejectData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", Args.CustomerID),
				new QueryParameter("Now", Args.DataAsOf)
			);

			MetaData.Validate();

			Trail.MyInputData.Init(Args, Cfg, MetaData, NewMarketplaces);
		} // GatherData

		private void CheckNumOfLoans() {
			if (Trail.MyInputData.NumOfOpenLoans >= Trail.MyInputData.AutoReRejectMaxAllowedLoans) {
				StepReject<OpenLoans>()
					.Init(Trail.MyInputData.NumOfOpenLoans, Trail.MyInputData.AutoReRejectMaxAllowedLoans);
			} else {
				StepNoDecision<OpenLoans>()
					.Init(Trail.MyInputData.NumOfOpenLoans, Trail.MyInputData.AutoReRejectMaxAllowedLoans);
			} // if
		} // CheckNumOfLoans

		private void CheckLastDecisionWasReject() {
			if (Trail.MyInputData.LastDecisionWasReject)
				StepNoDecision<LastDecisionWasReject>().Init(Trail.MyInputData.LastDecisionWasReject);
			else
				StepNoReject<LastDecisionWasReject>().Init(Trail.MyInputData.LastDecisionWasReject);
		} // CheckLastDecisionWasReject

		private void CheckNewMarketplaces() {
			if (Trail.MyInputData.NewDataSourceAdded)
				StepNoReject<MarketPlaceWasAdded>().Init(Trail.MyInputData.NewDataSourceAdded);
			else
				StepNoDecision<MarketPlaceWasAdded>().Init(Trail.MyInputData.NewDataSourceAdded);
		} // CheckNewMarketplaces

		private void CheckRejectionAge() {
			if (Trail.MyInputData.LastRejectDate == null) {
				StepNoDecision<LRDIsTooOld>().Init(-1, Trail.MyInputData.AutoReRejectMaxLRDAge);
				return;
			} // if

			decimal nAge = (decimal)(Trail.InputData.DataAsOf - Trail.MyInputData.LastRejectDate.Value).TotalDays;

			if (nAge > Trail.MyInputData.AutoReRejectMaxLRDAge)
				StepNoReject<LRDIsTooOld>().Init(nAge, Trail.MyInputData.AutoReRejectMaxLRDAge);
			else
				StepNoDecision<LRDIsTooOld>().Init(nAge, Trail.MyInputData.AutoReRejectMaxLRDAge);
		} // CheckRejectionAge

		private void CheckReturnRatio() {
			decimal nRatio = Trail.MyInputData.OpenLoansAmount < 0.00000000001m
				? 0
				: Trail.MyInputData.PrincipalRepaymentAmount / Trail.MyInputData.OpenLoansAmount;

			// Don't have open loan
			if (Trail.MyInputData.OpenLoansAmount < 0.00000000001m) {
				StepNoDecision<OpenLoansRepayments>().Init(
					Trail.MyInputData.OpenLoansAmount,
					Trail.MyInputData.PrincipalRepaymentAmount,
					Trail.MyInputData.AutoReRejectMinRepaidPortion
				);
			} else { // Have open loan
				if (nRatio < Trail.MyInputData.AutoReRejectMinRepaidPortion) {
					StepReject<OpenLoansRepayments>().Init(
						Trail.MyInputData.OpenLoansAmount,
						Trail.MyInputData.PrincipalRepaymentAmount,
						Trail.MyInputData.AutoReRejectMinRepaidPortion
					);
				} else {
					StepNoReject<OpenLoansRepayments>().Init(
						Trail.MyInputData.OpenLoansAmount,
						Trail.MyInputData.PrincipalRepaymentAmount,
						Trail.MyInputData.AutoReRejectMinRepaidPortion
					);
				} // if
			} // if
		} // CheckReturnRatio

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

			case RowType.Marketplace:
				NewMarketplaces.Add(sr.Fill<Marketplace>());
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ProcessRow

		private enum RowType {
			MetaData,
			Marketplace,
		} // enum RowType

		private T StepReject<T>() where T : ATrace {
			return Trail.Affirmative<T>(true);
		} // StepReject

		private T StepNoReject<T>() where T : ATrace {
			return Trail.Negative<T>(true);
		} // StepNoReject

		private T StepNoDecision<T>() where T : ATrace {
			return Trail.Dunno<T>();
		} // StepNoDecision
	} // class Agent
} // namespace
