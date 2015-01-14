namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using AutomationCalculator.Turnover;
	using Ezbob.Database;
	using Ezbob.Logger;

	/// <summary>
	///     Executes auto approval logic. Based on customer data and system calculated amount
	///     decides whether this amount should be approved.
	/// </summary>
	public class Agent {
		public virtual decimal ApprovedAmount { get; set; }

		public virtual DateTime Now { get; protected set; }

		public virtual Result Result { get; private set; }

		public virtual ApprovalTrail Trail { get; private set; }

		public Agent(
			int nCustomerID,
			decimal nSystemCalculatedAmount,
			AutomationCalculator.Common.Medal nMedal,
			AutomationCalculator.Common.MedalType medalType,
			AutomationCalculator.Common.TurnoverType? turnoverType,
			AConnection oDB,
			ASafeLog oLog
		) {
			Result = null;

			DB = oDB;
			Log = oLog ?? new SafeLog();
			Args = new Arguments(nCustomerID, nSystemCalculatedAmount, nMedal, medalType, turnoverType);
			this.m_oCheck = new Checker(this);
		} // constructor

		public virtual Agent Init() {
			Now = DateTime.UtcNow;

			ApprovedAmount = Args.SystemCalculatedAmount;

			MetaData = new MetaData();
			Payments = new List<Payment>();

			Funds = new AvailableFunds();
			WorstStatuses = new SortedSet<string>();

			Turnover = new AutoApprovalTurnover {
				TurnoverType = Args.TurnoverType,
			};
			Turnover.Init();

			OriginationTime = new OriginationTime(Log);

			Trail = new ApprovalTrail(Args.CustomerID, Log);
			Cfg = InitCfg();

			DirectorNames = new List<Name>();
			HmrcBusinessNames = new List<string>();

			return this;
		} // Init

		public virtual void MakeDecision() {
			Log.Debug("Secondary: checking if auto approval should take place for customer {0}...", Args.CustomerID);

			try {
				GatherData();
				this.m_oCheck.Run();
			} catch (Exception e) {
				Log.Error(e, "Exception during auto approval.");
				this.m_oCheck.StepForceFailed<ExceptionThrown>().Init(e);
			} // try

			if (Trail.HasDecided) {
				decimal minLoanAmount = Trail.MyInputData.Configuration.MinLoan;

				Trail.Amount = (int)(
					Math.Round(ApprovedAmount / minLoanAmount, 0, MidpointRounding.AwayFromZero) * minLoanAmount
				);

				Result = new Result((int)Trail.Amount, (int)MetaData.OfferLength, MetaData.IsEmailSendingBanned);
			} // if

			Log.Debug(
				"Secondary: checking if auto approval should take place for customer {0} complete; {1}\n{2}",
				Args.CustomerID,
				Trail,
				Result == null ? string.Empty : "Approved " + Result + "."
			);
		} // MakeDecision

		protected virtual Arguments Args { get; private set; }

		protected virtual Configuration Cfg { get; private set; }

		protected virtual AConnection DB { get; private set; }
		protected virtual List<Name> DirectorNames { get; private set; }

		protected virtual AvailableFunds Funds { get; private set; }

		protected virtual List<string> HmrcBusinessNames { get; private set; }

		protected virtual ASafeLog Log { get; private set; }
		protected virtual MetaData MetaData { get; private set; }
		protected virtual OriginationTime OriginationTime { get; private set; }

		protected virtual List<Payment> Payments { get; private set; }

		protected virtual AutoApprovalTurnover Turnover { get; private set; }

		protected virtual SortedSet<string> WorstStatuses { get; private set; }

		protected virtual void GatherAvailableFunds() {
			DB.GetFirst("GetAvailableFunds", CommandSpecies.StoredProcedure).Fill(Funds);
		} // GatherAvailableFunds

		/// <summary>
		///     Collects customer data from DB. Can be overridden to provide
		///     specific customer data instead of the current one.
		/// </summary>
		protected virtual void GatherData() {
			Cfg.Load();

			DB.ForEachRowSafe(
				ProcessRow,
				"LoadAutoApprovalData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", Args.CustomerID),
				new QueryParameter("Now", Now)
				);

			OriginationTime.FromExperian(MetaData.IncorporationDate);

			GatherAvailableFunds();

			Trail.MyInputData.FullInit(
				Now,
				Cfg,
				Args,
				MetaData,
				WorstStatuses,
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

		private enum RowType {
			MetaData,
			Payment,
			Cais,
			OriginationTime,
			Turnover,
			DirectorName,
			HmrcBusinessName,
		} // enum RowType

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

			case RowType.Payment:
				Payments.Add(sr.Fill<Payment>());
				break;

			case RowType.Cais:
				WorstStatuses.Add(sr["WorstStatus"]);
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
				HmrcBusinessNames.Add(AutomationCalculator.Utils.AdjustCompanyName(sr["Name"]));
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ProcessRow

		private readonly Checker m_oCheck;
	} // class Agent
} // namespace
