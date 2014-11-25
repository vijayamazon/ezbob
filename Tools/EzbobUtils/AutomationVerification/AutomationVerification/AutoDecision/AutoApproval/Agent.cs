namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.AutoApproval;
	using AutomationCalculator.ProcessHistory.Trails;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using Ezbob.Logger;

	/// <summary>
	/// Executes auto approval logic. Based on customer data and system calculated amount
	/// decides whether this amount should be approved.
	/// </summary>
	public class Agent {
		#region public

		#region constructor

		public Agent(int nCustomerID, decimal nSystemCalculatedAmount, AutomationCalculator.Common.Medal nMedal, AConnection oDB, ASafeLog oLog) {
			Result = null;

			DB = oDB;
			Log = oLog ?? new SafeLog();
			Args = new Arguments(nCustomerID, nSystemCalculatedAmount, nMedal);
		} // constructor

		#endregion constructor

		#region method Init

		public virtual Agent Init() {
			ApprovedAmount = Args.SystemCalculatedAmount;

			MetaData = new MetaData();
			Payments = new List<Payment>();

			Funds = new AvailableFunds();
			WorstStatuses = new SortedSet<string>();

			Turnover = new CalculatedTurnover();

			OriginationTime = new OriginationTime(Log);

			Trail = new ApprovalTrail(Args.CustomerID, Log);
			Cfg = new Configuration(DB, Log);

			return this;
		} // Init

		#endregion method Init

		#region method MakeDecision

		public virtual void MakeDecision() {
			Log.Debug("Checking if auto approval should take place for customer {0}...", Args.CustomerID);

			try {
				GatherData();

				// Once a step is not passed there is no need to continue result-wise. However the
				// process continues because we want to pick all the possible reasons for not
				// approving a customer in order to compare different implementations of the process.

				CheckInit();

				CheckMedal();
				CheckIsFraud();
				CheckIsBrokerCustomer();
				CheckTodayApprovedCount();
				CheckTodayOpenLoans();
				CheckOutstandingOffers();
				CheckAml();
				CheckCustomerStatus();
				CheckCompanyScore();
				CheckConsumerScore();
				CheckCustomerAge();
				CheckTurnovers();
				CheckCompanyAge();
				CheckDefaultAccounts();
				CheckTotalLoanCount();
				CheckCaisStatuses(MetaData.TotalLoanCount > 0
					? Cfg.GetAllowedCaisStatusesWithLoan()
					: Cfg.GetAllowedCaisStatusesWithoutLoan()
				);
				CheckRollovers();
				CheckLatePayments();
				CheckCustomerOpenLoans();
				CheckRepaidRatio();
				ReduceOutstandingPrincipal();
				CheckAllowedRange();
				CheckComplete();
			}
			catch (Exception e) {
				Log.Error(e, "Exception during auto approval.");
				StepFailed<ExceptionThrown>().Init(e);
			} // try

			if (Trail.HasDecided)
				Result = new Result((int)ApprovedAmount, (int)MetaData.OfferLength, MetaData.IsEmailSendingBanned);

			Log.Debug(
				"Checking if auto approval should take place for customer {0} complete; {1}\n{2}",
				Args.CustomerID,
				Trail,
				Result == null ? string.Empty : "Approved " + Result + "."
			);
		} // MakeDecision

		#endregion method MakeDecision

		public virtual Result Result { get; private set; }

		public virtual ApprovalTrail Trail { get; private set; }

		#endregion public

		#region protected

		#region method GatherData

		/// <summary>
		/// Collects customer data from DB. Can be overridden to provide
		/// specific customer data instead of the current one.
		/// </summary>
		protected virtual void GatherData() {
			Now = DateTime.UtcNow;

			Cfg.Load();

			DB.ForEachRowSafe(
				ProcessRow,
				"LoadAutoApprovalData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", Args.CustomerID),
				new QueryParameter("Now", Now)
			);

			OriginationTime.FromExperian(MetaData.IncorporationDate);

			DB.GetFirst("GetAvailableFunds", CommandSpecies.StoredProcedure).Fill(Funds);

			Trail.MyInputData.FullInit(
				DateTime.UtcNow,
				Cfg,
				Args,
				MetaData,
				WorstStatuses,
				Payments,
				OriginationTime,
				Turnover,
				Funds
			);

			MetaData.Validate();
		} // GatherData

		#endregion method GatherData

		#region properties

		protected virtual DateTime Now { get; set; }

		protected virtual AConnection DB { get; private set;}
		protected virtual ASafeLog Log { get; private set;}

		protected virtual Configuration Cfg { get; private set;}
		protected virtual Arguments Args { get; private set;}

		protected virtual MetaData MetaData { get; private set;}
		protected virtual List<Payment> Payments { get; private set;}

		protected virtual SortedSet<string> WorstStatuses { get; private set;}

		protected virtual OriginationTime OriginationTime { get; private set;}

		protected virtual decimal ApprovedAmount { get; set;}

		protected virtual CalculatedTurnover Turnover { get; private set;}

		protected virtual AvailableFunds Funds { get; private set;}

		#endregion properties

		#endregion protected

		#region private

		#region steps

		#region method CheckMedal

		private void CheckMedal() {
			if (Args.Medal == AutomationCalculator.Common.Medal.NoClassification)
				StepFailed<MedalIsGood>().Init(Args.Medal);
			else
				StepDone<MedalIsGood>().Init(Args.Medal);
		} // CheckMedal

		#endregion method CheckMedal

		#region method CheckInit

		private void CheckInit() {
			if ((ApprovedAmount > 0) && (MetaData.ValidationErrors.Count == 0))
				StepDone<InitialAssignment>().Init(ApprovedAmount, MetaData.ValidationErrors);
			else
				StepFailed<InitialAssignment>().Init(ApprovedAmount, MetaData.ValidationErrors);
		} // CheckInit

		#endregion method CheckInit

		#region method CheckIsFraud

		private void CheckIsFraud() {
			if (MetaData.FraudStatus == FraudStatus.Ok)
				StepDone<FraudSuspect>().Init(MetaData.FraudStatus);
			else
				StepFailed<FraudSuspect>().Init(MetaData.FraudStatus);
		} // CheckIsFraud

		#endregion method CheckIsFraud

		#region method CheckIsBrokerCustomer

		private void CheckIsBrokerCustomer() {
			if (MetaData.IsBrokerCustomer)
				StepFailed<IsBrokerCustomer>().Init();
			else
				StepDone<IsBrokerCustomer>().Init();
		} // CheckIsBrokerCustomer

		#endregion method CheckIsBrokerCustomer

		#region method CheckTodayApprovedCount

		private void CheckTodayApprovedCount() {
			if (MetaData.NumOfTodayAutoApproval > Cfg.MaxDailyApprovals)
				StepFailed<TodayApprovalCount>().Init(MetaData.NumOfTodayAutoApproval, Cfg.MaxDailyApprovals);
			else
				StepDone<TodayApprovalCount>().Init(MetaData.NumOfTodayAutoApproval, Cfg.MaxDailyApprovals);
		} // CheckTodayApprovedCount

		#endregion method CheckTodayApprovedCount

		#region method CheckTodayOpenLoans

		private void CheckTodayOpenLoans() {
			if (MetaData.TodayLoanSum > Cfg.MaxTodayLoans)
				StepFailed<TodayLoans>().Init(MetaData.TodayLoanSum, Cfg.MaxTodayLoans);
			else
				StepDone<TodayLoans>().Init(MetaData.TodayLoanSum, Cfg.MaxTodayLoans);
		} // CheckTodayOpenLoans

		#endregion method CheckTodayOpenLoans

		#region method CheckOutstandingOffers

		private void CheckOutstandingOffers() {
			if (Funds.Reserved > Cfg.MaxOutstandingOffers)
				StepFailed<OutstandingOffers>().Init(Funds.Reserved, Cfg.MaxOutstandingOffers);
			else
				StepDone<OutstandingOffers>().Init(Funds.Reserved, Cfg.MaxOutstandingOffers);
		} // CheckOutstandingOffers

		#endregion method CheckOutstandingOffers

		#region method CheckAml

		private void CheckAml() {
			if (0 == string.Compare(MetaData.AmlResult, "passed", StringComparison.InvariantCultureIgnoreCase))
				StepDone<AmlCheck>().Init(MetaData.AmlResult);
			else
				StepFailed<AmlCheck>().Init(MetaData.AmlResult);
		} // CheckAml

		#endregion method CheckAml

		#region method CheckCustomerStatus

		private void CheckCustomerStatus() {
			if (MetaData.CustomerStatusEnabled)
				StepDone<CustomerStatus>().Init(MetaData.CustomerStatusName);
			else
				StepFailed<CustomerStatus>().Init(MetaData.CustomerStatusName);
		} // CheckCustomerStatus

		#endregion method CheckCustomerStatus

		#region method CheckCompanyScore

		private void CheckCompanyScore() {
			if ((MetaData.CompanyScore <= 0) || (MetaData.CompanyScore >= Cfg.BusinessScoreThreshold))
				StepDone<BusinessScore>().Init(MetaData.CompanyScore, Cfg.BusinessScoreThreshold);
			else
				StepFailed<BusinessScore>().Init(MetaData.CompanyScore, Cfg.BusinessScoreThreshold);
		} // CheckCompayScore

		#endregion method CheckCompanyScore

		#region method CheckConsumerScore

		private void CheckConsumerScore() {
			if (MetaData.ConsumerScore >= Cfg.ExperianScoreThreshold)
				StepDone<ConsumerScore>().Init(MetaData.ConsumerScore, Cfg.ExperianScoreThreshold);
			else
				StepFailed<ConsumerScore>().Init(MetaData.ConsumerScore, Cfg.ExperianScoreThreshold);
		} // CheckConsumerScore

		#endregion method CheckConsumerScore

		#region method CheckCustomerAge

		private void CheckCustomerAge() {
			// nAge: full number of years in customer's age.
			int nAge = Now.Year - MetaData.DateOfBirth.Year;

			if (MetaData.DateOfBirth.AddYears(nAge) > Now) // this happens if customer had no birthday this year.
				nAge--;

			if ((Cfg.CustomerMinAge <= nAge) && (nAge <= Cfg.CustomerMaxAge))
				StepDone<Age>().Init(nAge, Cfg.CustomerMinAge, Cfg.CustomerMaxAge);
			else
				StepFailed<Age>().Init(nAge, Cfg.CustomerMinAge, Cfg.CustomerMaxAge);
		} // CheckCustomerAge

		#endregion method CheckCustomerAge

		#region method CheckTurnovers

		private void CheckTurnovers() {
			var oThresholds = new SortedDictionary<int, Tuple<decimal, TimePeriodEnum>> {
				{  1, new Tuple<decimal, TimePeriodEnum>(Cfg.MinTurnover1M, TimePeriodEnum.Month)  },
				{  3, new Tuple<decimal, TimePeriodEnum>(Cfg.MinTurnover3M, TimePeriodEnum.Month3) },
				{ 12, new Tuple<decimal, TimePeriodEnum>(Cfg.MinTurnover1Y, TimePeriodEnum.Year)   },
			};

			foreach (var pair in oThresholds) {
				decimal nTurnover = Turnover[pair.Key];
				decimal nThreshold = pair.Value.Item1;
				TimePeriodEnum nPeriod = pair.Value.Item2;

				AThresholdTrace oTrace = null;

				switch (nPeriod) {
				case TimePeriodEnum.Month:
					oTrace = (nTurnover > nThreshold) ? StepDone<OneMonthTurnover>() : StepFailed<OneMonthTurnover>();
					break;

				case TimePeriodEnum.Month3:
					oTrace = (nTurnover > nThreshold) ? StepDone<ThreeMonthsTurnover>() : StepFailed<ThreeMonthsTurnover>();
					break;

				case TimePeriodEnum.Year:
					oTrace = (nTurnover > nThreshold) ? StepDone<OneYearTurnover>() : StepFailed<OneYearTurnover>();
					break;

				default:
					throw new ArgumentOutOfRangeException();
				} // switch

				oTrace.Init(nTurnover, nThreshold);
			} // for each
		} // CheckTurnovers

		#endregion method CheckTurnovers

		#region method CheckCompanyAge

		private void CheckCompanyAge() {
			Log.Msg("Customer {0} business seniority: {1}.", Args.CustomerID, OriginationTime);

			if (OriginationTime.Since == null)
				StepFailed<MarketplaceSeniority>().Init(-1, Cfg.MinMPSeniorityDays);
			else {
				int nAge = (int)(DateTime.UtcNow - OriginationTime.Since.Value).TotalDays;

				if (nAge >= Cfg.MinMPSeniorityDays)
					StepDone<MarketplaceSeniority>().Init(nAge, Cfg.MinMPSeniorityDays);
				else
					StepFailed<MarketplaceSeniority>().Init(nAge, Cfg.MinMPSeniorityDays);
			} // if
		} // CheckCompanyAge

		#endregion method CheckCompanyAge

		#region method CheckDefaultAccounts

		private void CheckDefaultAccounts() {
			if (MetaData.NumOfDefaultAccounts > 0)
				StepFailed<DefaultAccounts>().Init(MetaData.NumOfDefaultAccounts);
			else
				StepDone<DefaultAccounts>().Init(MetaData.NumOfDefaultAccounts);
		} // CheckDefaultAccounts

		#endregion method CheckDefaultAccounts

		#region method CheckTotalLoanCount

		private void CheckTotalLoanCount() {
			StepDone<TotalLoanCount>().Init(MetaData.TotalLoanCount);
		} // CheckTotalLoanCount

		#endregion method CheckTotalLoanCount

		#region method CheckCaisStatuses

		private void CheckCaisStatuses(List<string> oAllowedStatuses) {
			List<string> diff = WorstStatuses.Except(oAllowedStatuses).ToList();

			if (diff.Count > 1)
				StepFailed<WorstCaisStatus>().Init(diff, WorstStatuses.ToList(), oAllowedStatuses);
			else
				StepDone<WorstCaisStatus>().Init(null, WorstStatuses.ToList(), oAllowedStatuses);
		} // CheckCaisStatuses

		#endregion method CheckCaisStatuses

		#region method CheckRollovers

		private void CheckRollovers() {
			if (MetaData.NumOfRollovers > 0)
				StepFailed<Rollovers>().Init();
			else
				StepDone<Rollovers>().Init();
		} // CheckRollovers

		#endregion method CheckRollovers

		#region method CheckLatePayments

		private void CheckLatePayments() {
			bool bHasLatePayments = false;

			foreach (Payment lp in Payments) {
				if (lp.IsLate(Cfg.MaxAllowedDaysLate)) {
					bHasLatePayments = true;

					lp.Fill(StepFailed<LatePayment>(), Cfg.MaxAllowedDaysLate);
				} // if
			} // for each

			if (!bHasLatePayments)
				StepDone<LatePayment>().Init(0, 0, Now, 0, Now, Cfg.MaxAllowedDaysLate);
		} // CheckLatePayments

		#endregion method CheckLatePayments

		#region method CheckCustomerOpenLoans

		private void CheckCustomerOpenLoans() {
			if (MetaData.OpenLoanCount > Cfg.MaxNumOfOutstandingLoans)
				StepFailed<OutstandingLoanCount>().Init(MetaData.OpenLoanCount, Cfg.MaxNumOfOutstandingLoans);
			else
				StepDone<OutstandingLoanCount>().Init(MetaData.OpenLoanCount, Cfg.MaxNumOfOutstandingLoans);
		} // CheckCustomerOpenLoans

		#endregion method CheckCustomerOpenLoans

		#region method CheckRepaidRatio

		private void CheckRepaidRatio() {
			decimal nRatio = MetaData.RepaidRatio;

			if (nRatio >= Cfg.MinRepaidPortion)
				StepDone<OutstandingRepayRatio>().Init(nRatio, Cfg.MinRepaidPortion);
			else
				StepFailed<OutstandingRepayRatio>().Init(nRatio, Cfg.MinRepaidPortion);
		} // CheckRepaidRatio

		#endregion method CheckRepaidRatio

		#region method ReduceOutstandingPrincipal

		private void ReduceOutstandingPrincipal() {
			ApprovedAmount -= MetaData.OutstandingPrincipal;

			StepDone<ReduceOutstandingPrincipal>().Init(MetaData.OutstandingPrincipal, ApprovedAmount);
		} // ReduceOutstandingPrincipal

		#endregion method ReduceOutstandingPrincipal

		#region method CheckAllowedRange

		private void CheckAllowedRange() {
			decimal nApprovedAmount = ApprovedAmount;

			if ((Cfg.MinAmount <= nApprovedAmount) && (nApprovedAmount <= Cfg.MaxAmount))
				StepDone<AmountOutOfRangle>().Init(nApprovedAmount, Cfg.MinAmount, Cfg.MaxAmount);
			else
				StepFailed<AmountOutOfRangle>().Init(nApprovedAmount, Cfg.MinAmount, Cfg.MaxAmount);
		} // CheckAllowedRange

		#endregion method CheckAllowedRange

		#region method CheckComplete

		private void CheckComplete() {
			decimal nApprovedAmount = ApprovedAmount;

			if (nApprovedAmount > 0)
				StepDone<Complete>().Init(nApprovedAmount);
			else
				StepFailed<Complete>().Init(nApprovedAmount);
		} // CheckComplete

		#endregion method CheckComplete

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
			Payment,
			Cais,
			OriginationTime,
			Turnover,
		} // enum RowType

		#endregion enum RowType

		#region method StepFailed

		private T StepFailed<T>() where T : ATrace {
			ApprovedAmount = 0;
			return Trail.Negative<T>(false);
		} // StepFailed

		#endregion method StepFailed

		#region method StepDone

		private T StepDone<T>() where T : ATrace {
			return Trail.Affirmative<T>(false);
		} // StepFailed

		#endregion method StepDone

		#endregion private
	} // class Agent
} // namespace
