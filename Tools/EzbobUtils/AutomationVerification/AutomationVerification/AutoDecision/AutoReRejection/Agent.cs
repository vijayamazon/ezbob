namespace AutomationCalculator.AutoDecision.AutoReRejection {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.ProcessHistory;
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

			}
			catch (Exception e) {
				Log.Error(e, "Exception during auto approval.");
				StepNoReject<ExceptionThrown>().Init(e);
			} // try

			Log.Debug(
				"Checking if auto re-reject should take place for customer {0} complete; {1}", Args.CustomerID, Trail
			);
		} // MakeDecision

		#endregion method MakeDecision

		public virtual ReRejectionTrail Trail { get; private set; }

		#endregion public

		#region properties

		protected virtual Arguments Args { get; private set; }
		protected virtual Configuration Cfg { get; private set; }
		protected virtual MetaData MetaData { get; private set; }
		protected virtual List<Marketplace> NewMarketplaces { get; private set; }

		protected virtual AConnection DB { get; private set; }
		protected virtual ASafeLog Log { get; private set; }

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

		#endregion properties

		#region private

		#region steps


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

		private T StepReject<T>() where T : ATrace {
			return Trail.Affirmative<T>(true);
		} // StepReject

		private T StepNoReject<T>() where T : ATrace {
			return Trail.Negative<T>(true);
		} // StepNoReject

		private T StepNoDecision<T>() where T : ATrace {
			return Trail.Dunno<T>();
		} // StepNoDecision

		#endregion private
	} // class Agent
} // namespace
