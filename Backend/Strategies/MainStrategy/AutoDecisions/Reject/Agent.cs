namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using AutomationCalculator.AutoDecision.AutoRejection;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using EzBob.Backend.Strategies.Experian;
	using EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.Turnover;
	using Ezbob.Backend.ModelsWithDB.Experian;
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
			UpdateErrors = new List<MpError>();

			return this;
		} // Init

		#endregion method Init

		public virtual RejectionTrail Trail { get; private set; }

		#region method MakeAndVerifyDecision

		public virtual bool MakeAndVerifyDecision() {
			RunPrimary();

			AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent oSecondary =
				RunSecondary();

			bool bSuccess = Trail.EqualsTo(oSecondary.Trail);

			Trail.Save(DB, oSecondary.Trail);

			return bSuccess;
		} // MakeAndVerifyDecision

		#endregion method MakeAndVerifyDecision

		#region method MakeDecision

		public virtual void MakeDecision(AutoDecisionRejectionResponse response) {
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

			OriginationTime.FromExperian(MetaData.IncorporationDate);

			Trail.MyInputData.Init(DateTime.UtcNow, ToInputData(), Cfg.Values);

			FillFromConsumerData();
			FillFromCompanyData();
		} // GatherData

		#endregion method GatherData

		#region fields

		protected virtual AConnection DB { get; private set; }
		protected virtual ASafeLog Log { get; private set; }

		protected virtual Configuration Cfg { get; private set; }
		protected virtual Arguments Args { get; private set; }
		protected virtual MetaData MetaData { get; private set; }

		protected virtual List<MpError> UpdateErrors { get; private set; }
		protected virtual CalculatedTurnover Turnover { get; private set; }
		protected virtual OriginationTime OriginationTime { get; private set; }

		#endregion fields

		#endregion protected

		#region private

		#region steps

		#endregion steps

		#region method RunPrimary

		private void RunPrimary() {
			Log.Debug("Checking if auto reject should take place for customer {0}...", Args.CustomerID);

			GatherData();

			Checker check = new Checker(this.Trail);

			check.WasApproved();
			check.HighAnnualTurnover();
			check.IsBroker();
			check.HighConsumerScore();
			check.HighBusinessScore();
			check.MpErrors();

			Trail.LockDecision();

			check.LowConsumerScore();
			check.LowBusinessScore();
			check.PersonalDefauls();
			check.BusinessDefaults();
			check.CompanyAge();
			check.CustomerStatus();
			check.CompanyFiles();
			check.LateAccounts();

			Log.Debug("Checking if auto reject should take place for customer {0} complete, {1}", Args.CustomerID, Trail);
		} // RunPrimary

		#endregion method RunPrimary

		#region method RunSecondary

		private AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent RunSecondary() {
			AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent oSecondary =
				new RejectionAgent(DB, Log, Args.CustomerID, Cfg.Values);

			oSecondary.MakeDecision(oSecondary.GetRejectionInputData(Trail.InputData.DataAsOf));

			return oSecondary;
		} // RunSecondary

		#endregion method RunSecondary

		#region method ToInputData

		private RejectionInputData ToInputData() {
			return new RejectionInputData {
				WasApproved = MetaData.ApprovedCrID > 0,
				IsBrokerClient = MetaData.BrokerID > 0,
				AnnualTurnover = Turnover.Annual,
				QuarterTurnover = Turnover.Quarter,
				ConsumerScore = MetaData.ConsumerScore,
				BusinessScore = MetaData.BusinessScore,
				HasMpError = UpdateErrors.Count > 0,
				HasCompanyFiles = MetaData.CompanyFilesCount > 0,
				CustomerStatus = MetaData.CustomerStatusName,

				NumOfDefaultConsumerAccounts = 0,
				DefaultAmountInConsumerAccounts = 0,
				NumOfDefaultBusinessAccounts = 0,
				DefaultAmountInBusinessAccounts = 0,
				NumOfLateConsumerAccounts = 0,
				ConsumerLateDays = 0,

				BusinessSeniorityDays = OriginationTime.Seniority,
			};
		} // ToInputData

		#endregion method ToInputData

		#region method FillFromConsumerData

		private void FillFromConsumerData() {
			var lcd = new LoadExperianConsumerData(Args.CustomerID, null, null, DB, Log);
			lcd.Execute();

			FillNumOfPersonalDefaults(lcd.Result);
			FillNumOfLateConsumerAccounts(lcd.Result);
		} // FillFromConsumerData

		#endregion method FillFromConsumerData

		#region method FillNumOfPersonalDefaults

		private void FillNumOfPersonalDefaults(ExperianConsumerData oData) {
			Trail.MyInputData.NumOfDefaultConsumerAccounts = 0;

			if ((oData == null) || (oData.Cais == null) || (oData.Cais.Count < 1))
				return;

			DateTime oThen = Trail.MyInputData.MonthsNumAgo;

			foreach (var cais in oData.Cais) {
				if (cais.Balance <= Cfg.Values.Reject_Defaults_Amount)
					continue;

				if (cais.MatchTo != 1)
					continue;
				
				if (!cais.LastUpdatedDate.HasValue)
					continue;

				if (string.IsNullOrWhiteSpace(cais.AccountStatusCodes))
					continue;

				DateTime cur = cais.LastUpdatedDate.Value;

				for (int i = 1; i <= cais.AccountStatusCodes.Length; i++) {
					if (cur < oThen)
						break;

					char status = cais.AccountStatusCodes[cais.AccountStatusCodes.Length - i];

					if ((status == '8') || (status == '9')) {
						Trail.MyInputData.NumOfDefaultConsumerAccounts++;
						break;
					} // if

					cur = cur.AddMonths(-1);
				} // for
			} // for each
		} // FillNumOfPersonalDefaults

		#endregion method FillNumOfPersonalDefaults

		#region method FillNumOfLateConsumerAccounts

		private void FillNumOfLateConsumerAccounts(ExperianConsumerData oData) {
			Trail.MyInputData.NumOfLateConsumerAccounts = 0;

			if ((oData == null) || (oData.Cais == null) || (oData.Cais.Count < 1))
				return;

			DateTime oMonthAgo = Trail.MyInputData.DataAsOf.AddMonths(-1);

			foreach (var cais in oData.Cais) {
				if (!cais.LastUpdatedDate.HasValue)
					continue;

				if (string.IsNullOrWhiteSpace(cais.AccountStatusCodes))
					continue;

				int nIdxToStartFrom = 1;

				int nMonthCount = Math.Min(Trail.MyInputData.Reject_LateLastMonthsNum, cais.AccountStatusCodes.Length);

				if (cais.LastUpdatedDate.Value > oMonthAgo) {
					nIdxToStartFrom = 2;
					nMonthCount = Math.Min(Trail.MyInputData.Reject_LateLastMonthsNum, cais.AccountStatusCodes.Length - 1);
				} // if

				for (int i = nIdxToStartFrom; i <= nMonthCount; i++) {
					char status = cais.AccountStatusCodes[cais.AccountStatusCodes.Length - i];

					if (!ms_oLateStatuses.Contains(status))
						continue;

					int nStatus = 0;

					int.TryParse(status.ToString(CultureInfo.InvariantCulture), out nStatus);

					if (nStatus > Trail.MyInputData.RejectionLastValidLate) {
						Trail.MyInputData.NumOfLateConsumerAccounts++;
						break;
					} // if
				} // for i
			} // for each account
		} // FillLateConsumerAccounts

		#endregion method FillNumOfLateConsumerAccounts

		#region method FillFromCompanyData

		private void FillFromCompanyData() {
			if (!MetaData.IsLtd || string.IsNullOrWhiteSpace(MetaData.CompanyRefNum))
				return;

			var ltd = new LoadExperianLtd(MetaData.CompanyRefNum, 0, DB, Log);
			ltd.Execute();

			if (ltd.Result == null)
				return;

			DateTime oThen = Trail.MyInputData.CompanyMonthsNumAgo;

			IEnumerable<ExperianLtdDL97> oDL97List = ltd.Result.GetChildren<ExperianLtdDL97>();

			foreach (var dl97 in oDL97List) {
				if (!dl97.DefaultBalance.HasValue)
					continue;

				if (dl97.DefaultBalance.Value <= Trail.MyInputData.Reject_Defaults_CompanyAmount)
					continue;

				if (string.IsNullOrWhiteSpace(dl97.AccountStatusLast12AccountStatuses))
					return;

				if (!dl97.CAISLastUpdatedDate.HasValue)
					continue;

				DateTime cur = dl97.CAISLastUpdatedDate.Value;

				for (int i = 1; i <= dl97.AccountStatusLast12AccountStatuses.Length; i++) {
					if (cur < oThen)
						break;

					char status = dl97.AccountStatusLast12AccountStatuses[dl97.AccountStatusLast12AccountStatuses.Length - i];

					if ((status == '8') || (status == '9')) {
						Trail.MyInputData.NumOfDefaultBusinessAccounts++;
						break;
					} // if

					cur = cur.AddMonths(-1);
				} // for
			} // for each account
		} // FillFromCompanyData

		#endregion method FillFromCompanyData

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
				UpdateErrors.Add(sr.Fill<MpError>());
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

		private static readonly SortedSet<char> ms_oLateStatuses = new SortedSet<char> { '1', '2', '3', '4', '5', '6', };

		#endregion private
	} // class Agent
} // namespace
