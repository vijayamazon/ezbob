namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using AutomationCalculator.AutoDecision.AutoRejection;
	using AutomationCalculator.AutoDecision.AutoRejection.Models;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoRejection;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using AutomationCalculator.Turnover;
	using ConfigManager;
	using Ezbob.Backend.Extensions;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Extensions;
	using Ezbob.Utils.Lingvo;

	public class Agent : AAutoDecisionBase {
		public virtual RejectionTrail Trail { get; private set; }

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
			HasApprovalChance = false;
			this.tag = tag;
		} // constructor

		public virtual Agent Init() {
			Trail = new RejectionTrail(
				Args.CustomerID,
				Args.CashRequestID,
				Args.NLCashRequestID,
				Log,
				CurrentValues.Instance.AutomationExplanationMailReciever,
				CurrentValues.Instance.MailSenderEmail,
				CurrentValues.Instance.MailSenderName
			);
			Trail.SetTag(this.tag);

			Now = DateTime.UtcNow;
			Cfg = InitCfg();
			MetaData = new MetaData();
			OriginationTime = new OriginationTime(Log);
			UpdateErrors = new List<MpError>();

			this.annualTurnover = 0;
			this.quarterTurnover = 0;

			return this;
		} // Init

		public bool HasApprovalChance { get; private set; }

		public override void MakeAndVerifyDecision() {
			AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent oSecondary = null;
			try {
				RunPrimary();

				oSecondary = RunSecondary();

				if (Trail.HasApprovalChance == oSecondary.Trail.HasApprovalChance) {
					Trail.Negative<SameApprovalChance>(false)
						.Init(Trail.HasApprovalChance, oSecondary.Trail.HasApprovalChance);
					oSecondary.Trail.Negative<SameApprovalChance>(false)
						.Init(Trail.HasApprovalChance, oSecondary.Trail.HasApprovalChance);
				} else {
					Trail.Affirmative<SameApprovalChance>(false)
						.Init(Trail.HasApprovalChance, oSecondary.Trail.HasApprovalChance);
					oSecondary.Trail.Affirmative<SameApprovalChance>(false)
						.Init(Trail.HasApprovalChance, oSecondary.Trail.HasApprovalChance);
				} // if

				WasMismatch = !Trail.EqualsTo(oSecondary.Trail);
			} catch (Exception e) {
				Log.Error(e, "Exception during auto rejection.");
				StepNoReject<ExceptionThrown>().Init(e);
			} // try

			Trail.Save(DB, oSecondary == null ? null : oSecondary.Trail);
		} // MakeAndVerifyDecision

		public override bool WasException {
			get {
				if (Trail == null)
					return false;

				return Trail.FindTrace<ExceptionThrown>() != null;
			} // get
		} // WasException

		public override bool AffirmativeDecisionMade {
			get { return Trail.HasDecided; }
		} // AffirmativeDecisionMade

		public virtual void RunPrimaryOnly() {
			RunPrimary();
		} // RunPrimaryOnly

		/// <summary>
		/// Calculate annual and quarter turnover for R
		/// </summary>
		/// <param name="customerId"></param>
		/// <param name="calculationTime"></param>
		public virtual void CalculateTurnoverForReject(int customerId, DateTime calculationTime) {
			try {
				this.annualTurnover = 0;
				this.quarterTurnover = 0;

				// all histories of customer that updateEnd is relevant
				List<MarketplaceTurnoverModel> h = DB.Fill<MarketplaceTurnoverModel>(
					"GetMarketplaceTurnoverByCustomerAndDate",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", customerId),
					new QueryParameter("Now", calculationTime)
				);

				if (h.Count < 1) {
					Log.Info(
						"Updating histories (customer {0}, @now {1}) not exists in MarketplaceTurnover view",
						customerId,
						calculationTime
					);
					return;
				} // if

				// all MP types of selection
				IEnumerable<Guid> mpTypes = h.Select(x => x.MarketplaceInternalID).Distinct();

				SortedSet<Guid> paymentAccounts = new SortedSet<Guid> (h
					.Where(x => x.IsPaymentAccount)
					.Select(x => x.MarketplaceInternalID)
					.Distinct()
				);

				List<FilteredAggregationResult> accounting = new List<FilteredAggregationResult>();
				List<FilteredAggregationResult> ecommerce = new List<FilteredAggregationResult>();
				List<FilteredAggregationResult> hmrc = new List<FilteredAggregationResult>();
				List<FilteredAggregationResult> bank = new List<FilteredAggregationResult>();
				List<FilteredAggregationResult> ebay = new List<FilteredAggregationResult>();
				List<FilteredAggregationResult> paypal = new List<FilteredAggregationResult>();

				foreach (var mpType in mpTypes) {
					if (mpType.Equals(MpType.Hmrc)) {
						IEnumerable<int> marketplaceIDs = h
							.Where(x => x.MarketplaceInternalID == MpType.Hmrc)
							.Select(x => x.CustomerMarketPlaceID)
							.Distinct();

						foreach (int mpID in marketplaceIDs) {
							List<MarketplaceTurnoverModel> thisMp = h.Where(x => x.CustomerMarketPlaceID == mpID).ToList();

							hmrc.AddRange(LastUpdatedEndHistoryTurnoversByMpType(
								thisMp,
								MpType.Hmrc,
								calculationTime,
								thisMp.Max(y => y.TheMonth)
							));
						} // for each marketplace

						//hmrc.ForEach(x => Log.Info(
						//	"HMRC: {0}, {1}, {2}, {3}",
						//	x.TheMonth,
						//	x.Turnover,
						//	x.MpId,
						//	x.Distance
						//));
					} else if (mpType.Equals(MpType.Yodlee)) {
						bank.AddRange(LastUpdatedEndHistoryTurnoversByMpType(h, MpType.Yodlee, calculationTime));
						// bank.ForEach(x => Log.Info(
						//	"bank: {0}, {1}, {2}, {3}",
						//	x.TheMonth,
						//	x.Turnover,
						//	x.MpId,
						//	x.Distance
						//));
					} else if (mpType.Equals(MpType.Ebay)) {
						ebay.AddRange(LastUpdatedEndHistoryTurnoversByMpType(h, MpType.Ebay, calculationTime));
						//ebay.ForEach(x => Log.Info(
						//	"ebay: {0}, {1}, {2}, {3}",
						//	x.TheMonth,
						//	x.Turnover,
						//	x.MpId,
						//	x.Distance
						//));
					} else if (mpType.Equals(MpType.PayPal)) {
						paypal.AddRange(LastUpdatedEndHistoryTurnoversByMpType(h, MpType.PayPal, calculationTime));
						// paypal.ForEach(x => Log.Info(
						//	"paypal: {0}, {1}, {2}, {3}",
						//	x.TheMonth,
						//	x.Turnover,
						//	x.MpId,
						//	x.Distance
						//));
					} else {
						// isPayment
						if (paymentAccounts.Contains(mpType))
							accounting.AddRange(LastUpdatedEndHistoryTurnoversByMpType(h, mpType, calculationTime));
						else
							ecommerce.AddRange(LastUpdatedEndHistoryTurnoversByMpType(h, mpType, calculationTime));
					} // if
				} // for each

				//accounting.ForEach(x => Log.Info(
				//	"accounting: {0}, {1}, {2}, {3}",
				//	x.TheMonth,
				//	x.Turnover,
				//	x.MpId,
				//	x.Distance
				//));

				//ecommerce.ForEach(x => Log.Info(
				//	"ecommerce: {0}, {1}, {2}, {3}",
				//	x.TheMonth,
				//	x.Turnover,
				//	x.MpId,
				//	x.Distance
				//));

				const int T1 = 1;
				const int T3 = 3;
				const int T6 = 6;
				decimal[] fillannual = { 0, 0, 0, 0 };

				// annualize and get max
				fillannual[0] = MaxAnnualizedTurnover(hmrc, T1, T3, T6);
				fillannual[1] = MaxAnnualizedTurnover(bank, T1, T3, T6);
				fillannual[2] = MaxAnnualizedTurnover(accounting, T1, T3, T6);

				// get all T for pp, evay, other ecommerce
				decimal[] tPaypal = CalculateTTurnover(paypal, T1, T3, T6);
				decimal[] tEbay = CalculateTTurnover(ebay, T1, T3, T6);
				decimal[] tEcommerce = CalculateTTurnover(ecommerce, T1, T3, T6);

				decimal[] annualEcommerce = { 0, 0, 0, 0 };
				annualEcommerce[0] = (tEcommerce[0] + Math.Max(tPaypal[0], tEbay[0])) * 12;
				annualEcommerce[1] = (tEcommerce[1] + Math.Max(tPaypal[1], tEbay[1])) * 4;
				annualEcommerce[2] = (tEcommerce[2] + Math.Max(tPaypal[2], tEbay[2])) * 2;
				annualEcommerce[3] = (tEcommerce[3] + Math.Max(tPaypal[3], tEbay[3]));

				fillannual[3] = annualEcommerce.Max();

				//fillannual.ForEach(c => Log.Info("==>MaxAnnualizedTurnover: {0}", c));

				decimal[] fillquarter = { 0, 0, 0, 0 };
				fillquarter[0] = MaxQuarterTurnover(hmrc, T1, T3);
				fillquarter[1] = MaxQuarterTurnover(bank, T1, T3);
				fillquarter[2] = MaxQuarterTurnover(accounting, T1, T3);
				fillquarter[3] = MaxQuarterTurnover(ecommerce, T1, T3) + Math.Max(
					MaxQuarterTurnover(paypal, T1, T3),
					MaxQuarterTurnover(ebay, T1, T3)
				);

				//fillquarter.ForEach(c => Log.Info("==>MaxQuarterTurnover: {0}", c));

				this.annualTurnover = fillannual.Max();
				this.quarterTurnover = fillquarter.Max();

				Log.Info(
					"customerID {0}, calculationDate {1}, annual: {2}, quarter: {3}",
					customerId,
					calculationTime,
					this.annualTurnover,
					this.quarterTurnover
				);
			} catch (Exception ex) {
				Log.Alert(
					ex,
					"Failed to calculate turnover for Reject, customerID {0}, calculationDate {1}",
					customerId,
					calculationTime
				);
			} // try
		} // CalculateTurnoverForReject

		protected virtual DateTime Now { get; set; }

		protected virtual AConnection DB { get; private set; }
		protected virtual ASafeLog Log { get; private set; }

		protected virtual Configuration Cfg { get; private set; }
		protected virtual Arguments Args { get; private set; }
		protected virtual MetaData MetaData { get; private set; }

		protected virtual List<MpError> UpdateErrors { get; private set; }
		protected virtual OriginationTime OriginationTime { get; private set; }

		protected virtual void RunPrimary() {
			Log.Debug("Primary: checking if auto reject should take place for customer {0}...", Args.CustomerID);

			GatherData();

			var checker = new Checker(Trail, Log).Run();

			HasApprovalChance = checker.Trail.HasApprovalChance;

			Trail.DecideIfNotDecided();

			Log.Debug(
				"Primary: checking if auto reject should take place for customer {0} complete, {1}",
				Args.CustomerID,
				Trail
			);
		} // RunPrimary

		protected virtual Configuration InitCfg() {
			return new Configuration(DB, Log);
		} // InitCfg

		protected virtual void LoadData() {
			DB.ForEachRowSafe(
				ProcessRow,
				"LoadAutoRejectData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", Args.CustomerID)
			);

			MetaData.Validate();

			CalculateTurnoverForReject(Args.CustomerID, Now);
		} // LoadData

		protected virtual ExperianConsumerData LoadConsumerData() {
			var lcd = new LoadExperianConsumerData(Args.CustomerID, null, null);
			lcd.Execute();

			return lcd.Result;
		} // LoadConsumerData

		protected virtual ExperianLtd LoadCompanyData() {
			var ltd = new LoadExperianLtd(MetaData.CompanyRefNum, 0);
			ltd.Execute();

			return ltd.Result;
		} // LoadCompanyData

		protected virtual void ProcessRow(SafeReader sr) {
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
				//Turnover.Add(sr, Log); neutral SP data collection (elinar)
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ProcessRow

		private enum RowType {
			MetaData,
			MpError,
			OriginationTime,
			Turnover,
		} // enum RowType

		private void GatherData() {
			Cfg.Load();

			LoadData();

			Trail.MyInputData.InitCfg(@Now, Cfg.Values);

			OriginationTime.FromExperian(MetaData.IncorporationDate);

			FillFromConsumerData();
			FillFromCompanyData();

			Trail.MyInputData.InitData(ToInputData());
		} // GatherData

		private void FillFromConsumerData() {
			var lcd = LoadConsumerData();

			var logList = new List<string>();

			this.numOfDefaultConsumerAccounts = lcd.FindNumOfPersonalDefaults(
				Cfg.Values.Reject_Defaults_Amount,
				Trail.MyInputData.MonthsNumAgo,
				logList
			);

			logList.ForEach(s => Log.Debug(s));

			FillNumOfLateConsumerAccounts(lcd);

			if (lcd == null)
				return;

			// Log.Debug("Consumer score before: {0}, bureau score '{1}'", MetaData.ConsumerScore, lcd.BureauScore);

			if (lcd.BureauScore != null)
				MetaData.ConsumerScore = MetaData.ConsumerScore.Max(lcd.BureauScore.Value);

			// Log.Debug("Consumer score after: {0}", MetaData.ConsumerScore);
		} // FillFromConsumerData

		private void FillFromCompanyData() {
			this.numOfDefaultBusinessAccounts = 0;

			if (!MetaData.IsLtd || string.IsNullOrWhiteSpace(MetaData.CompanyRefNum))
				return;

			var ltd = LoadCompanyData();

			if (ltd == null)
				return;

			DateTime oThen = Trail.MyInputData.CompanyMonthsNumAgo;

			// Log.Debug(
			// "DL97 searcher: then is '{0}'.",
			// oThen.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
			// );

			IEnumerable<ExperianLtdDL97> oDL97List = ltd.GetChildren<ExperianLtdDL97>();

			foreach (var dl97 in oDL97List) {
				// Log.Debug(
				// "DL97 entry: id '{0}', default balance '{1}', current balance '{4}', last updated '{2}', statuses '{3}'",
				// dl97.ID,
				// dl97.DefaultBalance.HasValue
				// ? dl97.DefaultBalance.Value.ToString(CultureInfo.InvariantCulture)
				// : "-- null --",
				// dl97.CAISLastUpdatedDate.HasValue
				// ? dl97.CAISLastUpdatedDate.Value.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
				// : "-- null --",
				// dl97.AccountStatusLast12AccountStatuses,
				// dl97.CurrentBalance.HasValue
				// ? dl97.CurrentBalance.Value.ToString(CultureInfo.InvariantCulture)
				// : "-- null --"
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
					// Log.Debug(
					// "DL97 id {0}: cur date is '{1}'.",
					// dl97.ID,
					// cur.ToString("d/MMM/yyy H:mm:ss", CultureInfo.InvariantCulture)
					// );

					if (cur < oThen) {
						// Log.Debug("DL97 id {0}: stopped looking for defaults - 'cur' is before 'then'.", dl97.ID);
						break;
					} // if

					char status =
						dl97.AccountStatusLast12AccountStatuses[dl97.AccountStatusLast12AccountStatuses.Length - i];

					// Log.Debug("DL97 id {0}: status[{1}] = '{2}'.", dl97.ID, i, status);

					if ((status == '8') || (status == '9')) {
						// Log.Debug("DL97 id {0}: is a default account.", dl97.ID);
						this.numOfDefaultBusinessAccounts++;
						break;
					} // if

					cur = cur.AddMonths(-1);
				} // for
			} // for each account
		} // FillFromCompanyData

		private AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent RunSecondary() {
			AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent oSecondary =
				new RejectionAgent(DB, Log, Args.CustomerID, Args.CashRequestID, Args.NLCashRequestID, Cfg.Values);

			oSecondary.MakeDecision(oSecondary.GetRejectionInputData(Trail.InputData.DataAsOf));

			return oSecondary;
		} // RunSecondary

		private RejectionInputData ToInputData() {
			return new RejectionInputData {
				WasApproved = MetaData.ApprovedCrID > 0,
				IsBrokerClient = MetaData.BrokerID > 0,
				AnnualTurnover = this.annualTurnover,
				QuarterTurnover = this.quarterTurnover,
				ConsumerScore = MetaData.ConsumerScore,
				BusinessScore = MetaData.BusinessScore,
				HasMpError = UpdateErrors.Count > 0,
				HasCompanyFiles = MetaData.CompanyFilesCount > 0,
				CustomerStatus = MetaData.CustomerStatusName,
				NumOfDefaultConsumerAccounts = this.numOfDefaultConsumerAccounts,
				DefaultAmountInConsumerAccounts = 0,
				NumOfDefaultBusinessAccounts = this.numOfDefaultBusinessAccounts,
				DefaultAmountInBusinessAccounts = 0,
				NumOfLateConsumerAccounts = this.numOfLateConsumerAccounts,
				ConsumerLateDays = 0,
				BusinessSeniorityDays = OriginationTime.Seniority,
				ConsumerDataTime = MetaData.ConsumerDataTime,
			};
		} // ToInputData

		private void FillNumOfLateConsumerAccounts(ExperianConsumerData oData) {
			this.numOfLateConsumerAccounts = 0;

			if ((oData == null) || (oData.Cais == null) || (oData.Cais.Count < 1))
				return;

			List<ExperianConsumerDataCais> lst = oData.Cais.Where(cais =>
				cais.LastUpdatedDate.HasValue &&
					(cais.MatchTo == 1) &&
					!string.IsNullOrWhiteSpace(cais.AccountStatusCodes) &&
					(cais.LastUpdatedDate.Value <= Trail.InputData.DataAsOf) &&
					(MiscUtils.CountMonthsBetween(cais.LastUpdatedDate.Value, Trail.InputData.DataAsOf) < 1)
				)
				.ToList();

			Log.Debug("Fill num of lates: {0} found.", Grammar.Number(lst.Count, "relevant account"));

			foreach (var cais in lst) {
				Log.Debug(
					"Fill num of lates cais id {0}: last updated = '{1}', match to = '{2}', statues = '{3}', now = {4}",
					cais.Id,
					cais.LastUpdatedDate.HasValue
					? cais.LastUpdatedDate.Value.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
					: "-- null --",
					cais.MatchTo.HasValue ? cais.MatchTo.Value.ToString(CultureInfo.InvariantCulture) : "-- null --",
					cais.AccountStatusCodes,
					Trail.InputData.DataAsOf.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
				);

				int nMonthCount = Math.Min(Trail.MyInputData.Reject_LateLastMonthsNum, cais.AccountStatusCodes.Length);

				Log.Debug(
					"Fill num of lates cais id {0}: month count is {1}, status count is {2}.",
					cais.Id,
					nMonthCount,
					cais.AccountStatusCodes.Length
				);

				for (int i = 1; i <= nMonthCount; i++) {
					char status = cais.AccountStatusCodes[cais.AccountStatusCodes.Length - i];

					Log.Debug("Fill num of lates cais id {0}: status[{1}] = '{2}'.", cais.Id, i, status);

					if (!lateStatuses.Contains(status)) {
						Log.Debug("Fill num of lates cais id {0} ain't no late: not a late status.", cais.Id);
						continue;
					} // if

					int nStatus = 0;

					int.TryParse(status.ToString(CultureInfo.InvariantCulture), out nStatus);

					Log.Debug("Fill num of lates cais id {0}: status[{1}] = '{2}'.", cais.Id, i, nStatus);

					if (nStatus > Trail.MyInputData.RejectionLastValidLate) {
						Log.Debug("Fill num of lates cais id {0} is late.", cais.Id);
						this.numOfLateConsumerAccounts++;
						break;
					} // if
				} // for i
			} // for each account
		} // FillLateConsumerAccounts

		private T StepNoReject<T>() where T : ATrace {
			return Trail.Negative<T>(true);
		} // StepNoReject

		private static decimal MaxAnnualizedTurnover(
			List<FilteredAggregationResult> list,
			int t1,
			int t3,
			int t6,
			int ec1 = 12,
			int ec3 = 4,
			int ec6 = 2
		) {
			decimal[] filltt = { 0, 0, 0, 0 };
			filltt[0] = list.Where(t => t.Distance < t1).Sum(t => t.Turnover) * ec1;
			filltt[1] = list.Where(t => t.Distance < t3).Sum(t => t.Turnover) * ec3;
			filltt[2] = list.Where(t => t.Distance < t6).Sum(t => t.Turnover) * ec6;
			filltt[3] = list.Sum(t => t.Turnover);
			decimal totalTurnover = filltt.Max();
			return totalTurnover < 0 ? 0 : totalTurnover;
		} // MaxAnnualizedTurnover

		private static decimal MaxQuarterTurnover(List<FilteredAggregationResult> list, int t1, int t3) {
			decimal[] filltt = { 0, 0 };
			filltt[0] = list.Where(t => t.Distance < t1).Sum(t => t.Turnover);
			filltt[1] = list.Where(t => t.Distance < t3).Sum(t => t.Turnover);
			decimal totalTurnover = filltt.Max();
			return totalTurnover < 0 ? 0 : totalTurnover;
		} // MaxQuarterTurnover

		private IEnumerable<FilteredAggregationResult> LastUpdatedEndHistoryTurnoversByMpType(
			List<MarketplaceTurnoverModel> inputList,
			Guid type,
			DateTime calculationTime,
			DateTime? lastExistingDataTime = null
		) {
			Log.Debug(
				"LastUpdatedEndHistoryTurnoversByMpType(type = '{0}', calculation time = '{1}', " +
				"last existing data time = '{2}') started.",
				type,
				calculationTime.MomentStr(),
				lastExistingDataTime.MomentStr()
			);

			List<MarketplaceTurnoverModel> ofcurrentType =
				inputList.Where(x => x.MarketplaceInternalID == type).ToList();

			if (ofcurrentType.Count < 1) {
				Log.Debug(
					"LastUpdatedEndHistoryTurnoversByMpType returns empty result: " +
					"no entries found in MarketplaceTurnover view with MP internal id '{0}'.",
					type
				);

				return Enumerable.Empty<FilteredAggregationResult>();
			} // if

			DateTime lastUpdateDate = ofcurrentType.Max(z => z.UpdatingEnd);

			DateTime periodStart = MiscUtils.GetPeriodAgo(
				calculationTime,
				lastUpdateDate,
				CurrentValues.Instance.TotalsMonthTail,
				lastExistingDataTime
			);

			DateTime periodEnd = periodStart.AddMonths(11);

			Log.Debug(
				"LastUpdatedEndHistoryTurnoversByMpType: " +
				"calculationTime = '{0}', lastUpdateDate = '{1}', period start = '{2}', period end = '{3}'.",
				calculationTime.MomentStr(),
				lastUpdateDate.MomentStr(),
				periodStart.MomentStr(),
				periodEnd.MomentStr()
			);

			List<MarketplaceTurnoverModel> histories =
				ofcurrentType.Where(z => z.TheMonth >= periodStart && z.TheMonth <= periodEnd).ToList();

			var os = new StringBuilder();

			histories.ForEach(x => os.AppendFormat(
				"\tMonth = {0}, turnover = {1}, MpHistoryID = {2}, MpID = {3}.\n",
				x.TheMonth,
				x.Turnover,
				x.CustomerMarketPlaceUpdatingHistoryID,
				x.CustomerMarketPlaceID
			));

			Log.Debug(
				"LastUpdatedEndHistoryTurnoversByMpType: filtered MarketplaceTurnover entries are\n{0}",
				histories.Count < 1 ? "\tnothing found.\n" : os.ToString()
			);

			if (histories.Count < 1) {
				Log.Debug(
					"LastUpdatedEndHistoryTurnoversByMpType returns empty result: " +
					"no filtered entries found in MarketplaceTurnover."
				);

				return Enumerable.Empty<FilteredAggregationResult>();
			} // if

			var groups = histories.GroupBy(ag => new {
				ag.CustomerMarketPlaceID,
				ag.TheMonth
			});

			var result = new List<FilteredAggregationResult>();

			foreach (var grp in groups) {
				MarketplaceTurnoverModel first = grp.OrderByDescending(p => p.AggID).First();

				var far = new FilteredAggregationResult {
					Distance = (11 - MiscUtils.DateDiffInMonths(periodStart, first.TheMonth)),
					TheMonth = first.TheMonth,
					MpId = first.CustomerMarketPlaceID,
					Turnover = first.Turnover
				};

				result.Add(far);
			} // for each group

			os.Clear();

			result.ForEach(x => os.AppendFormat(
				"\tMonth = {0}, Distance = {1}, Turnover = {2}, MpID = {3}.\n",
				x.TheMonth,
				x.Distance,
				x.Turnover,
				x.MpId
			));

			Log.Debug(
				"LastUpdatedEndHistoryTurnoversByMpType: result entries are\n{0}",
				result.Count < 1 ? "\tnothing found.\n" : os.ToString()
			);

			return result;
		} // LastUpdatedEndHistoryTurnoversByMpType

		private static decimal[] CalculateTTurnover(List<FilteredAggregationResult> list, int t1, int t3, int t6) {
			decimal[] filltt = { 0, 0, 0, 0 };

			filltt[0] = list.Where(t => t.Distance < t1).Sum(t => t.Turnover);
			filltt[1] = list.Where(t => t.Distance < t3).Sum(t => t.Turnover);
			filltt[2] = list.Where(t => t.Distance < t6).Sum(t => t.Turnover);
			filltt[3] = list.Sum(t => t.Turnover);

			return filltt;
		} // CalculateTTurnover

		private static readonly SortedSet<char> lateStatuses = new SortedSet<char> { '1', '2', '3', '4', '5', '6', };

		private int numOfDefaultConsumerAccounts;
		private int numOfLateConsumerAccounts;
		private int numOfDefaultBusinessAccounts;

		private decimal quarterTurnover;
		private decimal annualTurnover;
		private readonly string tag;
	} // class Agent
} // namespace
