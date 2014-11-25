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
		#region public

		#region constructor

		public Agent(int nCustomerID, DateTime? dataAsOf, AConnection oDB, ASafeLog oLog) {
			Log = oLog ?? new SafeLog();
			DB = oDB;

			Args = new Arguments(nCustomerID, dataAsOf);
		} // constructor

		#endregion constructor

		#region method Init

		public virtual Agent Init() {
			Trail = new ReRejectionTrail(Args.CustomerID, Log);

			Cfg = new Configuration(DB, Log);

			NewMarketplaces = new List<Marketplace>();
			MetaData = new MetaData();

			return this;
		} // Init

		#endregion method Init

		#region method MakeDecision

		public virtual void MakeDecision() {
			Log.Debug("Checking if auto re-reject should take place for customer {0}...", Args.CustomerID);

			try {
				GatherData();

				CheckHasBeenRejected();
				CheckNewMarketplaces();
				CheckRejectionAge();
				CheckLoanCount();

				if (Trail.MyInputData.HasLoans)
					CheckReturnRatio();
			}
			catch (Exception e) {
				Log.Error(e, "Exception during auto re-rejection.");
				StepNoReject<ExceptionThrown>().Init(e);
			} // try

			Log.Debug(
				"Checking if auto re-reject should take place for customer {0} complete; {1}", Args.CustomerID, Trail
			);
		} // MakeDecision

		#endregion method MakeDecision

		public virtual ReRejectionTrail Trail { get; private set; }

		#endregion public

		#region protected

		#region properties

		protected virtual Arguments Args { get; private set; }
		protected virtual Configuration Cfg { get; private set; }
		protected virtual MetaData MetaData { get; private set; }
		protected virtual List<Marketplace> NewMarketplaces { get; private set; }

		#region property Now

		protected virtual DateTime Now {
			get { return Trail.InputData.DataAsOf; }
		} // Now

		#endregion property Now

		protected virtual AConnection DB { get; private set; }
		protected virtual ASafeLog Log { get; private set; }

		#endregion properties

		#region method GatherData

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

		#endregion method GatherData

		#endregion protected

		#region private

		#region steps

		#region method CheckHasBeenRejected

		private void CheckHasBeenRejected() {
			if (Trail.MyInputData.WasManuallyRejected)
				StepNoDecision<WasRejected>().Init(Trail.MyInputData.WasManuallyRejected);
			else
				StepNoReject<WasRejected>().Init(Trail.MyInputData.WasManuallyRejected);
		} // CheckHasBeenRejected

		#endregion method CheckHasBeenRejected

		#region method CheckNewMarketplaces

		private void CheckNewMarketplaces() {
			if (Trail.MyInputData.NewDataSourceAdded)
				StepNoReject<MarketPlaceWasAdded>().Init(Trail.MyInputData.NewDataSourceAdded);
			else
				StepNoDecision<MarketPlaceWasAdded>().Init(Trail.MyInputData.NewDataSourceAdded);
		} // CheckNewMarketplaces

		#endregion method CheckNewMarketplaces

		#region method CheckRejectionAge

		private void CheckRejectionAge() {
			if (Trail.MyInputData.LastManualRejectDate == null) {
				StepNoDecision<LRDIsTooOld>().Init(-1, Trail.MyInputData.AutoReRejectMaxLRDAge);
				return;
			} // if

			decimal nAge = (decimal)(Trail.InputData.DataAsOf - Trail.MyInputData.LastManualRejectDate.Value).TotalDays;

			if (nAge > Trail.MyInputData.AutoReRejectMaxLRDAge)
				StepNoReject<LRDIsTooOld>().Init(nAge, Trail.MyInputData.AutoReRejectMaxLRDAge);
			else
				StepNoDecision<LRDIsTooOld>().Init(nAge, Trail.MyInputData.AutoReRejectMaxLRDAge);
		} // CheckRejectionAge

		#endregion method CheckRejectionAge

		#region method CheckLoanCount

		private void CheckLoanCount() {
			StepNoDecision<TotalLoanCount>().Init(MetaData.LoanCount);
		} // CheckLoanCount

		#endregion method CheckLoanCount

		#region method CheckReturnRatio

		private void CheckReturnRatio() {
			decimal nRatio = Trail.MyInputData.OpenLoansAmount < 0.00000000001m
				? 1
				: Trail.MyInputData.PrincipalRepaymentAmount / Trail.MyInputData.OpenLoansAmount;

			if (nRatio < Trail.MyInputData.AutoReRejectMinRepaidPortion) {
				StepReject<OpenLoansRepayments>().Init(
					Trail.MyInputData.OpenLoansAmount,
					Trail.MyInputData.PrincipalRepaymentAmount,
					Trail.MyInputData.AutoReRejectMinRepaidPortion
				);
			}
			else{
				StepNoDecision<OpenLoansRepayments>().Init(
					Trail.MyInputData.OpenLoansAmount,
					Trail.MyInputData.PrincipalRepaymentAmount,
					Trail.MyInputData.AutoReRejectMinRepaidPortion
				);
			}
		} // CheckReturnRatio

		#endregion method CheckReturnRatio

		#endregion steps

		#region method ProcessRow

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

		#endregion method ProcessRow

		#region enum RowType

		private enum RowType {
			MetaData,
			Marketplace,
		} // enum RowType

		#endregion enum RowType

		#region method StepReject

		private T StepReject<T>() where T : ATrace {
			return Trail.Affirmative<T>(true);
		} // StepReject

		#endregion method StepReject

		#region method StepNoReject

		private T StepNoReject<T>() where T : ATrace {
			return Trail.Negative<T>(true);
		} // StepNoReject

		#endregion method StepNoReject

		#region method StepNoDecision

		private T StepNoDecision<T>() where T : ATrace {
			return Trail.Dunno<T>();
		} // StepNoDecision

		#endregion method StepNoDecision

		#endregion private
	} // class Agent
} // namespace
