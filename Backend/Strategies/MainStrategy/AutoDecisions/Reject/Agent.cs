namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject {
	using System;
	using AutomationCalculator.AutoDecision.AutoRejection;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.Turnover;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class Agent {
		#region public

		#region constructor

		public Agent(int nCustomerID, AConnection oDB, ASafeLog oLog) {
			DB = oDB;
			Log = oLog ?? new SafeLog();
			Args = new Arguments(nCustomerID);
		} // constructor

		#endregion constructor

		#region method Init

		public virtual Agent Init() {
			Trail = new RejectionTrail(Args.CustomerID, Log);

			Cfg = new Configuration(DB, Log);
			MetaData = new MetaData();
			Turnover = new CalculatedTurnover();
			OriginationTime = new OriginationTime(Log);

			return this;
		} // Init

		#endregion method Init

		public virtual RejectionTrail Trail { get; private set; }

		#region method MakeAndVerifyDecision

		public virtual bool MakeAndVerifyDecision() {
			GatherData();

			// TODO: check steps

			AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent oSecondary =
				new RejectionAgent(DB, Log, Args.CustomerID /*, TODO: configs */);

			oSecondary.MakeDecision(oSecondary.GetRejectionInputData(Trail.InputData.DataAsOf));

			bool bSuccess = Trail.EqualsTo(oSecondary.Trail);

			Trail.Save(DB, oSecondary.Trail);

			return bSuccess;
		} // MakeAndVerifyDecision

		#endregion method MakeAndVerifyDecision

		#region method MakeDecision

		public virtual void MakeDecision(AutoDecisionRejectionResponse response) {
			Log.Debug("Checking if auto reject should take place for customer {0}...", Args.CustomerID);

			try {
				MakeAndVerifyDecision();

				if (Trail.HasDecided) {
					response.CreditResult = "Rejected";
					response.UserStatus = "Rejected";
					response.SystemDecision = "Reject";
					response.DecisionName = "Rejection";
					response.DecidedToReject = true;
				} // if
			}
			catch (Exception e) {
				Log.Error(e, "Exception during auto rejection.");
				StepNoReject<ExceptionThrown>().Init(e);
			} // try

			Log.Debug("Checking if auto reject should take place for customer {0} complete.", Args.CustomerID);

			Log.Msg("{0}", Trail);
		} // MakeDecision

		#endregion method MakeDecision

		#endregion public

		#region protected

		#region method GatherData

		protected virtual void GatherData() {
			Cfg.Load();

			DB.ForEachRowSafe(
				ProcessRow,
				"LoadAutoRejectData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", Args.CustomerID)
			);

			MetaData.Validate();

			// TODO: OriginationTime.FromExperian(MetaData.IncorporationDate);

			// TODO: Trail.MyInputData.Init(DateTime.UtcNow, null);
		} // GatherData

		#endregion method GatherData

		#region fields

		protected virtual AConnection DB { get; private set; }
		protected virtual ASafeLog Log { get; private set; }

		protected virtual Configuration Cfg { get; private set; }
		protected virtual Arguments Args { get; private set; }
		protected virtual MetaData MetaData { get; private set; }

		protected virtual CalculatedTurnover Turnover { get; private set; }
		protected virtual OriginationTime OriginationTime { get; private set; }

		#endregion fields

		#endregion protected

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

			case RowType.MpError:
				// TODO
				break;

			case RowType.OriginationTime:
				OriginationTime.Process(sr);
				break;

			case RowType.Turnover:
				Turnover.Add(sr, Log);
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ProcessRow

		#endregion method ProcessRow

		#region enum RowType

		private enum RowType {
			MetaData,
			MpError,
			OriginationTime,
			Turnover,
		} // enum RowType

		#endregion enum RowType

		#region method StepNoReject

		private T StepNoReject<T>() where T : ATrace {
			return Trail.Negative<T>(true);
		} // StepNoReject

		#endregion method StepNoReject

		#region method StepReject

		private T StepReject<T>() where T : ATrace {
			return Trail.Affirmative<T>(true);
		} // StepReject

		#endregion method StepReject

		#region method StepNoDecision

		private T StepNoDecision<T>() where T : ATrace {
			return Trail.Dunno<T>();
		} // StepNoDecision

		#endregion method StepReject

		#endregion private
	} // class Agent
} // namespace
