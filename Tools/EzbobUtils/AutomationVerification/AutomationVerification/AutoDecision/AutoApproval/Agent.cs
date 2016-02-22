namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using AutomationCalculator.Turnover;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;

	/// <summary>
	///     Executes auto approval logic. Based on customer data and system calculated amount
	///     decides whether this amount should be approved.
	/// </summary>
	public class Agent {
		public virtual AConnection DB { get; private set; }

		public virtual DateTime Now { get; protected set; }

		public virtual ApprovalTrail Trail { get; private set; }

		public Agent(
			int nCustomerID,
			long? cashRequestID,
			long? nlCashRequestID,
			decimal nSystemCalculatedAmount,
			AutomationCalculator.Common.Medal nMedal,
			AutomationCalculator.Common.MedalType medalType,
			AutomationCalculator.Common.TurnoverType? turnoverType,
			AConnection oDB,
			ASafeLog oLog
		) {
			DB = oDB;
			Log = oLog.Safe();
			Args = new Arguments(
				nCustomerID,
				cashRequestID,
				nlCashRequestID,
				nSystemCalculatedAmount,
				nMedal,
				medalType,
				turnoverType
			);
			this.caisAccounts = new List<ExperianConsumerDataCaisAccounts>();
		} // constructor

		public virtual Agent Init() {
			Trail = new ApprovalTrail(Args.CustomerID, Args.CashRequestID, Args.NlCashRequestID, Log) {
				Amount = Args.SystemCalculatedAmount,
			};

			using (Trail.AddCheckpoint(ProcessCheckpoints.Initializtion)) {
				Now = DateTime.UtcNow;

				MetaData = new MetaData();
				Payments = new List<Payment>();

				Funds = new AvailableFunds();

				Turnover = new AutoApprovalTurnover {
					TurnoverType = Args.TurnoverType,
				};
				Turnover.Init();

				OriginationTime = new OriginationTime(Log);

				Cfg = InitCfg();

				DirectorNames = new List<Name>();
				HmrcBusinessNames = new List<NameForComparison>();
			} // using timer step

			return this;
		} // Init

		public virtual void MakeDecision() {
			using (Trail.AddCheckpoint(ProcessCheckpoints.MakeDecision)) {
				Log.Debug("Secondary: checking if auto approval should take place for customer {0}...", Args.CustomerID);

				Checker check = CreateChecker();

				try {
					using (Trail.AddCheckpoint(ProcessCheckpoints.GatherData))
						GatherData();

					using (Trail.AddCheckpoint(ProcessCheckpoints.RunCheck))
						check.Run();
				} catch (Exception e) {
					Log.Error(e, "Exception during auto approval.");
					check.StepForceFailed<ExceptionThrown>().Init(e);
				} // try

				string logMsg = string.Empty;

				if (Trail.HasDecided) {
					logMsg = string.Format(
						"Approved amount {0} for {1}, email banned: {2}",
						Trail.RoundedAmount,
						Grammar.Number(MetaData.OfferLength, "day"),
						MetaData.EmailSendingBanned ? "yes" : "no"
					);
				} // if

				Log.Debug(
					"Secondary: checking if auto approval should take place for customer {0} complete; {1}\n{2}",
					Args.CustomerID,
					Trail,
					logMsg
				);
			} // using timer step
		} // MakeDecision

		public virtual IEnumerable<string> FindBadCaisStatuses() {
			return CaisAccounts
				.Where(ca => ExperianConsumerDataCaisAccountsExt.IsBad(
					Now,
					ca.LastUpdatedDate,
					Math.Max(ca.Balance ?? 0, ca.CurrentDefBalance ?? 0),
					ca.AccountStatusCodes
				))
				.Select(ca => string.Format(
					"ID {0}, updated on {1}, balance {2}, codes {3}",
					ca.Id,
					(ca.LastUpdatedDate ?? DateTime.UtcNow).ToString("d-MMM-yyyy", CultureInfo.InvariantCulture),
					Math.Max(ca.Balance ?? 0, ca.CurrentDefBalance ?? 0),
					ca.AccountStatusCodes
				));
		} // FindBadCaisStatuses

		protected virtual Checker CreateChecker() {
			return new Checker(this);
		} // CreateChecker

		protected virtual Arguments Args { get; private set; }

		protected virtual Configuration Cfg { get; private set; }

		protected virtual List<Name> DirectorNames { get; private set; }

		protected virtual AvailableFunds Funds { get; private set; }

		protected virtual List<NameForComparison> HmrcBusinessNames { get; private set; }

		protected internal virtual ASafeLog Log { get; private set; }
		protected virtual MetaData MetaData { get; private set; }
		protected virtual OriginationTime OriginationTime { get; private set; }

		protected virtual List<Payment> Payments { get; private set; }

		protected virtual AutoApprovalTurnover Turnover { get; private set; }

		protected virtual void GatherAvailableFunds() {
			DB.GetFirst("GetAvailableFunds", CommandSpecies.StoredProcedure).Fill(Funds);
		} // GatherAvailableFunds

		/// <summary>
		/// Collects customer data from DB. Can be overridden to provide
		/// specific customer data instead of the current one.
		/// </summary>
		protected virtual void GatherData() {
			this.caisAccounts.Clear();

			Cfg.Load();

			DB.ForEachRowSafe(
				ProcessRow,
				"LoadAutoApprovalData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", Args.CustomerID),
				new QueryParameter("Now", Now)
			);

			MetaData.NumOfDefaultAccounts = ExperianConsumerDataExt.FindNumOfPersonalDefaults(
				this.caisAccounts, 
				Cfg.Reject_Defaults_Amount,
				Now.AddMonths(-1 * Cfg.Reject_Defaults_MonthsNum)
			);

			OriginationTime.FromExperian(MetaData.IncorporationDate);

			GatherAvailableFunds();

			Trail.MyInputData.FullInit(
				Now,
				Cfg,
				Args,
				MetaData,
				Payments,
				OriginationTime,
				Turnover,
				Funds,
				DirectorNames,
				HmrcBusinessNames
			);

			MetaData.Validate();
		} // GatherData

		protected virtual Configuration InitCfg() {
			return new Configuration(DB, Log);
		} // InitCfg

		protected enum RowType {
			MetaData,
			Payment,
			OriginationTime,
			Turnover,
			DirectorName,
			HmrcBusinessName,
			ExperianConsumerDataCais,
			CompanyDissolutionDate,
		} // enum RowType

		protected virtual RowType? LastRowType { get; set; }

		protected virtual void ProcessRow(SafeReader sr) {
			LastRowType = null;

			RowType nRowType;

			string sRowType = sr.ContainsField("DatumType", "RowType") ? sr["DatumType"] : sr["RowType"];

			if (!Enum.TryParse(sRowType, out nRowType)) {
				Log.Alert("Unsupported row type encountered: '{0}'.", sRowType);
				return;
			} // if

			LastRowType = nRowType;

			Log.Debug("Auto approve agent, processing input row of type {0}.", sRowType);

			switch (nRowType) {
			case RowType.MetaData:
				sr.Fill(MetaData);
				break;

			case RowType.Payment:
				Payments.Add(sr.Fill<Payment>());
				break;

			case RowType.OriginationTime:
				OriginationTime.Process(sr);
				break;

			case RowType.Turnover:
				Turnover.Add(sr.Fill<TurnoverDbRow>());
				break;

			case RowType.DirectorName:
				DirectorNames.Add(new Name(sr["FirstName"], sr["LastName"]));
				break;

			case RowType.HmrcBusinessName:
				if (sr["BelongsToCustomer"])
					HmrcBusinessNames.Add(new NameForComparison(sr["Name"]));
				break;

			case RowType.ExperianConsumerDataCais:
				this.caisAccounts.Add(sr.Fill<ExperianConsumerDataCaisAccounts>());
				break;

			case RowType.CompanyDissolutionDate:
				MetaData.CompanyDissolutionDate = sr["CompanyDissolutionDate"];
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ProcessRow

		protected virtual List<ExperianConsumerDataCaisAccounts> CaisAccounts { get { return this.caisAccounts; } }

		private readonly List<ExperianConsumerDataCaisAccounts> caisAccounts;
	} // class Agent
} // namespace
