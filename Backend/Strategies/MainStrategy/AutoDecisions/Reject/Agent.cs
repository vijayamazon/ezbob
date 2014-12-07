namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoRejection;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using DbConstants;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Backend.Strategies.Experian;
	using EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.Turnover;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Extensions;
	using Ezbob.Utils.Lingvo;

	public class Agent {
		#region public

		public virtual RejectionTrail Trail { get; private set; }

		#region constructor

		public Agent(int nCustomerID, AConnection oDB, ASafeLog oLog) {
			DB = oDB;
			Log = oLog ?? new SafeLog();
			Args = new Arguments(nCustomerID);
		} // constructor

		#endregion constructor

		#region method Init

		public virtual Agent Init() {
			Trail = new RejectionTrail(Args.CustomerID, Log, CurrentValues.Instance.AutomationExplanationMailReciever, CurrentValues.Instance.MailSenderEmail, CurrentValues.Instance.MailSenderName);

			Now = DateTime.UtcNow;
			Cfg = InitCfg();
			MetaData = new MetaData();
			Turnover = new CalculatedTurnover();
			OriginationTime = new OriginationTime(Log);
			UpdateErrors = new List<MpError>();

			return this;
		} // Init

		#endregion method Init

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

		public virtual void MakeDecision(AutoDecisionResponse response) {
			bool bSuccess = false;

			try {
				bSuccess = MakeAndVerifyDecision();
			}
			catch (Exception e) {
				Log.Error(e, "Exception during auto rejection.");
				StepNoReject<ExceptionThrown>().Init(e);
			} // try

			if (bSuccess && Trail.HasDecided) {
				response.CreditResult = CreditResultStatus.Rejected;
				response.UserStatus = Status.Rejected;
				response.SystemDecision = SystemDecision.Reject;
				response.DecisionName = "Rejection";
				response.Decision = DecisionActions.Reject;
			} // if
		} // MakeDecision

		#endregion method MakeDecision

		#endregion public

		#region protected

		#region properties

		protected virtual DateTime Now { get; set; }

		protected virtual AConnection DB { get; private set; }
		protected virtual ASafeLog Log { get; private set; }

		protected virtual Configuration Cfg { get; private set; }
		protected virtual Arguments Args { get; private set; }
		protected virtual MetaData MetaData { get; private set; }

		protected virtual List<MpError> UpdateErrors { get; private set; }
		protected virtual CalculatedTurnover Turnover { get; private set; }
		protected virtual OriginationTime OriginationTime { get; private set; }

		#endregion properties

		#region method RunPrimary

		protected virtual void RunPrimary() {
			Log.Debug("Primary: checking if auto reject should take place for customer {0}...", Args.CustomerID);

			GatherData();

			new Checker(this.Trail, this.Log).Run();

			Log.Debug("Primary: checking if auto reject should take place for customer {0} complete, {1}", Args.CustomerID, Trail);
		} // RunPrimary

		#endregion method RunPrimary

		#region method InitCfg

		protected virtual Configuration InitCfg() {
			return new Configuration(DB, Log);
		} // InitCfg

		#endregion method InitCfg

		#region method LoadData

		protected virtual void LoadData() {
			DB.ForEachRowSafe(
				ProcessRow,
				"LoadAutoRejectData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", Args.CustomerID)
			);

			MetaData.Validate();
		} // LoadData

		#endregion method LoadMetaData

		#region method LoadConsumerData

		protected virtual ExperianConsumerData LoadConsumerData() {
			var lcd = new LoadExperianConsumerData(Args.CustomerID, null, null, DB, Log);
			lcd.Execute();

			return lcd.Result;
		} // LoadConsumerData

		#endregion method LoadConsumerData

		#region method LoadCompanyData

		protected virtual ExperianLtd LoadCompanyData() {
			var ltd = new LoadExperianLtd(MetaData.CompanyRefNum, 0, DB, Log);
			ltd.Execute();

			return ltd.Result;
		} // LoadCompanyData

		#endregion method LoadCompanyData

		#region method ProcessRow

		protected  virtual void ProcessRow(SafeReader sr) {
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

		#endregion protected

		#region private

		#region method GatherData

		private void GatherData() {
			Cfg.Load();

			LoadData();

			Trail.MyInputData.InitCfg(@Now, Cfg.Values);

			OriginationTime.FromExperian(MetaData.IncorporationDate);

			FillFromConsumerData();
			FillFromCompanyData();

			Trail.MyInputData.InitData(ToInputData());
		} // GatherData

		#endregion method GatherData

		#region method FillFromConsumerData

		private void FillFromConsumerData() {
			var lcd = LoadConsumerData();

			FillNumOfPersonalDefaults(lcd);
			FillNumOfLateConsumerAccounts(lcd);

			if (lcd == null)
				return;

			Log.Debug("Consumer score before: {0}, bureau score '{1}'", MetaData.ConsumerScore, lcd.BureauScore);

			if (lcd.BureauScore != null)
				MetaData.ConsumerScore = MetaData.ConsumerScore.Max(lcd.BureauScore.Value);

			Log.Debug("Consumer score after: {0}", MetaData.ConsumerScore);
		} // FillFromConsumerData

		#endregion method FillFromConsumerData

		#region method FillFromCompanyData

		private void FillFromCompanyData() {
			m_nNumOfDefaultBusinessAccounts = 0;

			if (!MetaData.IsLtd || string.IsNullOrWhiteSpace(MetaData.CompanyRefNum))
				return;

			var ltd = LoadCompanyData();

			if (ltd == null)
				return;

			DateTime oThen = Trail.MyInputData.CompanyMonthsNumAgo;

			// Log.Debug("DL97 searcher: then is '{0}'.", oThen.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture));

			IEnumerable<ExperianLtdDL97> oDL97List = ltd.GetChildren<ExperianLtdDL97>();

			foreach (var dl97 in oDL97List) {
				// Log.Debug(
					// "DL97 entry: id '{0}', default balance '{1}', current balance '{4}', last updated '{2}', statuses '{3}'",
					// dl97.ID,
					// dl97.DefaultBalance.HasValue ? dl97.DefaultBalance.Value.ToString(CultureInfo.InvariantCulture) : "-- null --",
					// dl97.CAISLastUpdatedDate.HasValue ? dl97.CAISLastUpdatedDate.Value.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture) : "-- null --",
					// dl97.AccountStatusLast12AccountStatuses,
					// dl97.CurrentBalance.HasValue ? dl97.CurrentBalance.Value.ToString(CultureInfo.InvariantCulture) : "-- null --"
				// );

				decimal nBalance = Math.Max(dl97.DefaultBalance ?? 0, dl97.CurrentBalance ?? 0);

				// Log.Debug("DL97 id {0} balance is {1}.", dl97.ID, nBalance);

				if (nBalance <= Trail.MyInputData.Reject_Defaults_CompanyAmount) {
					// Log.Debug("DL97 id {0} ain't not default account: low balance.", dl97.ID);
					continue;
				} // if

				if (!dl97.CAISLastUpdatedDate.HasValue) {
					// Log.Debug("DL97 id {0} ain't not default account: no updated time.", dl97.ID);
					continue;
				} // if

				if (string.IsNullOrWhiteSpace(dl97.AccountStatusLast12AccountStatuses)) {
					// Log.Debug("DL97 id {0} ain't not default account: no statuses.", dl97.ID);
					continue;
				} // if

				DateTime cur = dl97.CAISLastUpdatedDate.Value;

				for (int i = 1; i <= dl97.AccountStatusLast12AccountStatuses.Length; i++) {
					// Log.Debug("DL97 id {0}: cur date is '{1}'.", dl97.ID, cur.ToString("d/MMM/yyy H:mm:ss", CultureInfo.InvariantCulture));

					if (cur < oThen) {
						// Log.Debug("DL97 id {0}: stopped looking for defaults - 'cur' is before 'then'.", dl97.ID);
						break;
					} // if

					char status = dl97.AccountStatusLast12AccountStatuses[dl97.AccountStatusLast12AccountStatuses.Length - i];

					// Log.Debug("DL97 id {0}: status[{1}] = '{2}'.", dl97.ID, i, status);

					if ((status == '8') || (status == '9')) {
						// Log.Debug("DL97 id {0}: is a default account.", dl97.ID);
						m_nNumOfDefaultBusinessAccounts++;
						break;
					} // if

					cur = cur.AddMonths(-1);
				} // for
			} // for each account
		} // FillFromCompanyData

		#endregion method FillFromCompanyData

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

				NumOfDefaultConsumerAccounts = m_nNumOfDefaultConsumerAccounts,
				DefaultAmountInConsumerAccounts = 0,
				NumOfDefaultBusinessAccounts = m_nNumOfDefaultBusinessAccounts,
				DefaultAmountInBusinessAccounts = 0,
				NumOfLateConsumerAccounts = m_nNumOfLateConsumerAccounts,
				ConsumerLateDays = 0,

				BusinessSeniorityDays = OriginationTime.Seniority,

				ConsumerDataTime = MetaData.ConsumerDataTime,
			};
		} // ToInputData

		#endregion method ToInputData

		#region method FillNumOfPersonalDefaults

		private void FillNumOfPersonalDefaults(ExperianConsumerData oData) {
			m_nNumOfDefaultConsumerAccounts = 0;

			if ((oData == null) || (oData.Cais == null) || (oData.Cais.Count < 1))
				return;

			var lst = oData.Cais.Where(cais => {
				decimal nBalance = Math.Max(cais.CurrentDefBalance ?? 0, cais.Balance ?? 0);

				return
					(nBalance > Cfg.Values.Reject_Defaults_Amount) &&
					(cais.MatchTo == 1) &&
					cais.LastUpdatedDate.HasValue &&
					!string.IsNullOrWhiteSpace(cais.AccountStatusCodes);
			}).ToList();

			Log.Debug("Fill personal defaults: {0} found.", Grammar.Number(lst.Count, "relevant account"));

			DateTime oThen = Trail.MyInputData.MonthsNumAgo;

			Log.Debug("Fill personal defaults: interested in data after {0}.", oThen.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture));

			foreach (var cais in lst) {
				// ReSharper disable PossibleInvalidOperationException
				Log.Debug(
					"Fill personal defaults cais {0}: updated on {1}, statuses is '{2}', now is {3}",
					cais.Id,
					cais.LastUpdatedDate.Value.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
					cais.AccountStatusCodes,
					Trail.InputData.DataAsOf.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
				);

				DateTime cur = cais.LastUpdatedDate.Value;
				// ReSharper restore PossibleInvalidOperationException

				for (int i = 1; i <= cais.AccountStatusCodes.Length; i++) {
					if (cur < oThen) {
						Log.Debug("Fill personal defaults cais {0} ain't no default: cur ({1}) is too early.", cais.Id, cur.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture));
						break;
					} // if

					char status = cais.AccountStatusCodes[cais.AccountStatusCodes.Length - i];

					Log.Debug("Fill personal defaults cais {0} ain't no default: status[{1}] = '{2}' cur ({3}).", cais.Id, i, status, cur.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture));

					if ((status == '8') || (status == '9')) {
						Log.Debug("Fill personal defaults cais {0} is default.", cais.Id);
						m_nNumOfDefaultConsumerAccounts++;
						break;
					} // if

					cur = cur.AddMonths(-1);
				} // for
			} // for each
		} // FillNumOfPersonalDefaults

		#endregion method FillNumOfPersonalDefaults

		#region method FillNumOfLateConsumerAccounts

		private void FillNumOfLateConsumerAccounts(ExperianConsumerData oData) {
			m_nNumOfLateConsumerAccounts = 0;

			if ((oData == null) || (oData.Cais == null) || (oData.Cais.Count < 1))
				return;

			List<ExperianConsumerDataCais> lst = oData.Cais.Where(cais =>
				cais.LastUpdatedDate.HasValue &&
				(cais.MatchTo == 1) &&
				!string.IsNullOrWhiteSpace(cais.AccountStatusCodes) &&
				(cais.LastUpdatedDate.Value <= Trail.InputData.DataAsOf) &&
				(MiscUtils.CountMonthsBetween(cais.LastUpdatedDate.Value, Trail.InputData.DataAsOf) < 1)
			).ToList();

			Log.Debug("Fill num of lates: {0} found.", Grammar.Number(lst.Count, "relevant account"));

			foreach (var cais in lst) {
				Log.Debug(
					"Fill num of lates cais id {0}: last updated = '{1}', match to = '{2}', statues = '{3}', now = {4}",
					cais.Id,
					cais.LastUpdatedDate.HasValue ? cais.LastUpdatedDate.Value.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture) : "-- null --",
					cais.MatchTo.HasValue ? cais.MatchTo.Value.ToString(CultureInfo.InvariantCulture) : "-- null --",
					cais.AccountStatusCodes,
					Trail.InputData.DataAsOf.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
				);

				int nMonthCount = Math.Min(Trail.MyInputData.Reject_LateLastMonthsNum, cais.AccountStatusCodes.Length);

				Log.Debug("Fill num of lates cais id {0}: month count is {1}, status count is {2}.", cais.Id, nMonthCount, cais.AccountStatusCodes.Length);

				for (int i = 1; i <= nMonthCount; i++) {
					char status = cais.AccountStatusCodes[cais.AccountStatusCodes.Length - i];

					Log.Debug("Fill num of lates cais id {0}: status[{1}] = '{2}'.", cais.Id, i, status);

					if (!ms_oLateStatuses.Contains(status)) {
						Log.Debug("Fill num of lates cais id {0} ain't no late: not a late status.", cais.Id);
						continue;
					} // if

					int nStatus = 0;

					int.TryParse(status.ToString(CultureInfo.InvariantCulture), out nStatus);

					Log.Debug("Fill num of lates cais id {0}: status[{1}] = '{2}'.", cais.Id, i, nStatus);

					if (nStatus > Trail.MyInputData.RejectionLastValidLate) {
						Log.Debug("Fill num of lates cais id {0} is late.", cais.Id);
						m_nNumOfLateConsumerAccounts++;
						break;
					} // if
				} // for i
			} // for each account
		} // FillLateConsumerAccounts

		#endregion method FillNumOfLateConsumerAccounts

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

		private int m_nNumOfDefaultConsumerAccounts;
		private int m_nNumOfLateConsumerAccounts;
		private int m_nNumOfDefaultBusinessAccounts;

		#endregion private
	} // class Agent
} // namespace
