namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using System.Data;
	using Ezbob.Logger;
	
	public class Rejection
	{
		private int _autoRejectionExceptionAnualTurnover;
		private int _rejectDefaultsCreditScore;
		private int _rejectDefaultsCompanyScore;
		private int _rejectMinimalSeniority;
		private int _lowCreditScore;
		private int _rejectDefaultsAccountsNum;
		private int _rejectDefaultsCompanyAccountsNum;
		private int _autoRejectionExceptionCreditScore;
		private int _errorMPsNum;
		private int _loanOfferApprovalNum;
		private int _numOfDefaultAccounts;
		private int _numOfCompanyDefaultAccounts;
		private readonly AConnection _db;
		private readonly ASafeLog _log;
		private readonly double _totalSumOfOrders1YTotalForRejection;
		private readonly double _totalSumOfOrders3MTotalForRejection;
		private readonly double _yodlee1YForRejection;
		private readonly double _yodlee3MForRejection;
		private readonly double _marketplaceSeniorityDays;
		private readonly bool _enableAutomaticRejection;
		private int _lowTotalAnnualTurnover;
		private int _lowTotalThreeMonthTurnover;
		private readonly double _maxExperianConsumerScore;
		private readonly int _customerId;
		private readonly int _maxCompanyScore;
		private readonly bool _customerStatusIsEnabled;
		private readonly bool _customerStatusIsWarning;
		private readonly bool _isBrokerCustomer;
		private readonly bool _isLimitedCompany;
		private readonly int _companySeniorityDays;
		private readonly bool _isOffline;
		private bool _hasHmrc;
		private decimal _hmrcAnnualRevenues;
		private decimal _hmrcQuarterRevenues;
		private bool _hasYodlee;
		private decimal _lowOfflineQuarterRevenue;
		private decimal _lowOfflineAnnualRevenue;
		private int _numOfLateAccounts;
		private int _rejectNumOfLateAccounts;
		private int _payPalNumberOfStores;
		private decimal _payPalTotalSumOfOrders3M;
		private decimal _payPalTotalSumOfOrders1Y;
		private readonly string _customerStatusName;
		private readonly List<AutoDecisionCondition> _conditions;

		public Rejection(
			List<AutoDecisionCondition> conditions,
			int customerId,
			double totalSumOfOrders1YTotalForRejection,
			double totalSumOfOrders3MTotalForRejection,
			double yodlee1YForRejection,
			double yodlee3MForRejection,
			double marketplaceSeniorityDays,
			bool enableAutomaticRejection,
			double maxExperianConsumerScore,
			int maxCompanyScore,
			bool customerStatusIsEnabled,
			bool customerStatusIsWarning,
			bool isBrokerCustomer,
			bool isLimitedCompany,
			int companySeniorityDays,
			bool isOffline,
			string customerStatusName,
			AConnection oDb, ASafeLog oLog)
		{
			_db = oDb;
			_log = oLog;
			_totalSumOfOrders1YTotalForRejection = totalSumOfOrders1YTotalForRejection;
			_totalSumOfOrders3MTotalForRejection = totalSumOfOrders3MTotalForRejection;
			_yodlee1YForRejection = yodlee1YForRejection;
			_yodlee3MForRejection = yodlee3MForRejection;
			_marketplaceSeniorityDays = marketplaceSeniorityDays;
			_enableAutomaticRejection = enableAutomaticRejection;
			_maxExperianConsumerScore = maxExperianConsumerScore;
			_customerId = customerId;
			_maxCompanyScore = maxCompanyScore;
			_customerStatusIsEnabled = customerStatusIsEnabled;
			_customerStatusIsWarning = customerStatusIsWarning;
			_isBrokerCustomer = isBrokerCustomer;
			_isLimitedCompany = isLimitedCompany;
			_companySeniorityDays = companySeniorityDays;
			_isOffline = isOffline;
			_customerStatusName = customerStatusName;
			_conditions = conditions;
		}

		private bool IsException(out string reason)
		{
			reason = "AutoReject: Rejection exception";

			// Customers that have been approved at least once before (even if the latest decision was rejection) and in enabled status/fraud suspect
			bool conditionValue = _loanOfferApprovalNum > 0 && _customerStatusIsEnabled && !_customerStatusIsWarning;
			string conditionDescription = string.Format("Exception - Has past approval and customer status is enabled and not warning (_loanOfferApprovalNum > 0 && _customerStatusIsEnabled && !_customerStatusIsWarning) [{0} > 0 && {1} && !{2}]", _loanOfferApprovalNum, _customerStatusIsEnabled, _customerStatusIsWarning);
			_conditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue});
			if (conditionValue)
			{
				reason = string.Format("{0} : {1}", reason, "Customers that have been approved at least once before");
			}
			// Annual turnover above 250,000
			conditionValue = _totalSumOfOrders1YTotalForRejection > _autoRejectionExceptionAnualTurnover;
			conditionDescription = string.Format("Exception - High annual turnover (_totalSumOfOrders1YTotalForRejection > _autoRejectionExceptionAnualTurnover) [{0} > {1}]", _totalSumOfOrders1YTotalForRejection, _autoRejectionExceptionAnualTurnover);
			_conditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
			if (conditionValue)
			{
				reason = string.Format("{0} : {1} {2} ({3})", reason, "Annual turnover above", _autoRejectionExceptionAnualTurnover, _totalSumOfOrders1YTotalForRejection);
			}
			// Consumer score (max of applicant and directors) above 800
			conditionValue = _maxExperianConsumerScore > _autoRejectionExceptionCreditScore;
			conditionDescription = string.Format("Exception - High max personal score (_maxExperianConsumerScore > _autoRejectionExceptionCreditScore) [{0} > {1}]", _maxExperianConsumerScore, _autoRejectionExceptionCreditScore);
			_conditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
			if (conditionValue)
			{
				reason = string.Format("{0} : {1} {2} ({3})", reason, "Consumer score above", _autoRejectionExceptionCreditScore, _maxExperianConsumerScore);
			}
			// Company score (max of company, parent companies) (default >= 40)
			int rejectionExceptionMaxCompanyScore = CurrentValues.Instance.RejectionExceptionMaxCompanyScore;
			conditionValue = _maxCompanyScore >= rejectionExceptionMaxCompanyScore;
			conditionDescription = string.Format("Exception - High max company score (_maxCompanyScore >= rejectionExceptionMaxCompanyScore) [{0} >= {1}]", _maxCompanyScore, rejectionExceptionMaxCompanyScore);
			_conditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
			if (conditionValue)
			{
				reason = string.Format("{0} : {1} {2} ({3})", reason, "Company score above", rejectionExceptionMaxCompanyScore, _maxCompanyScore);
			}
			// MP with error AND (consumer score > 500 OR company score > 10)
			int rejectionExceptionMaxConsumerScoreForMpError = CurrentValues.Instance.RejectionExceptionMaxConsumerScoreForMpError;
			int rejectionExceptionMaxCompanyScoreForMpError = CurrentValues.Instance.RejectionExceptionMaxCompanyScoreForMpError;
			conditionValue = _errorMPsNum > 0 && (_maxExperianConsumerScore > rejectionExceptionMaxConsumerScoreForMpError || _maxCompanyScore > rejectionExceptionMaxCompanyScoreForMpError);
			conditionDescription = string.Format("Exception - MP with error and high enough experian score (_errorMPsNum > 0 && (_maxExperianConsumerScore > rejectionExceptionMaxConsumerScoreForMpError || _maxCompanyScore > rejectionExceptionMaxCompanyScoreForMpError)) [{0} > 0 && ({1} > {2} || {3} > {4})]", _errorMPsNum, _maxExperianConsumerScore, rejectionExceptionMaxConsumerScoreForMpError, _maxCompanyScore, rejectionExceptionMaxCompanyScoreForMpError);
			_conditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
			if (conditionValue)
			{
				reason = string.Format("{0} : {1} {2} {3}", reason, "MP with error and experian scores", _maxExperianConsumerScore, _maxCompanyScore);
			}
			// Couldn't get experian score
			conditionValue = (decimal)_maxExperianConsumerScore == 0;
			conditionDescription = string.Format("Exception - No personal score (_maxExperianConsumerScore == 0) [{0} == 0]", _maxExperianConsumerScore);
			_conditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
			if (conditionValue)
			{
				reason = string.Format("{0} : {1}", reason, "Experian consumer score is 0");
			}
			// Customer via broker
			conditionValue = _isBrokerCustomer;
			conditionDescription = string.Format("Exception - Customer via broker (_isBrokerCustomer) [{0}]", _isBrokerCustomer);
			_conditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
			if (conditionValue) // TODO: Currently rejections are disabled for broker customers - this logic is in contradiction to it (should be refactored)
			{
				reason = string.Format("{0} : {1}", reason, "Customer via broker");
			}

			if (reason != "AutoReject: Rejection exception")
			{
				return true;
			}

			reason = "No rejection exception";
			_log.Info("customerId {0} {1}", _customerId, reason);
			return false;
		}

		private void Init()
		{
			_autoRejectionExceptionAnualTurnover = CurrentValues.Instance.AutoRejectionException_AnualTurnover;
			_rejectDefaultsCreditScore = CurrentValues.Instance.Reject_Defaults_CreditScore;
			_rejectDefaultsCompanyScore = CurrentValues.Instance.Reject_Defaults_CompanyScore;
			_rejectMinimalSeniority = CurrentValues.Instance.Reject_Minimal_Seniority;
			_lowCreditScore = CurrentValues.Instance.LowCreditScore;
			_rejectDefaultsAccountsNum = CurrentValues.Instance.Reject_Defaults_AccountsNum;
			_rejectDefaultsCompanyAccountsNum = CurrentValues.Instance.Reject_Defaults_CompanyAccountsNum;
			_autoRejectionExceptionCreditScore = CurrentValues.Instance.AutoRejectionException_CreditScore;
			int rejectDefaultsMonths = CurrentValues.Instance.Reject_Defaults_MonthsNum;
			int rejectDefaultsAmount = CurrentValues.Instance.Reject_Defaults_Amount;
			_lowTotalAnnualTurnover = CurrentValues.Instance.TotalAnnualTurnover;
			_lowTotalThreeMonthTurnover = CurrentValues.Instance.TotalThreeMonthTurnover;
			_lowOfflineAnnualRevenue =  CurrentValues.Instance.Reject_LowOfflineAnnualRevenue;
			_lowOfflineQuarterRevenue = CurrentValues.Instance.Reject_LowOfflineQuarterRevenue;
			_rejectNumOfLateAccounts = CurrentValues.Instance.Reject_NumOfLateAccounts;
			int rejectDefaultsCompanyMonths = CurrentValues.Instance.Reject_Defaults_CompanyMonthsNum;
			int rejectDefaultsCompanyAmount = CurrentValues.Instance.Reject_Defaults_CompanyAmount;

			DataTable dt = _db.ExecuteReader(
				"GetCustomerRejectionData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", _customerId),
				new QueryParameter("Reject_Defaults_Months", rejectDefaultsMonths),
				new QueryParameter("Reject_Defaults_Amount", rejectDefaultsAmount),
				new QueryParameter("Reject_Defaults_CompanyMonths", rejectDefaultsCompanyMonths),
				new QueryParameter("Reject_Defaults_CompanyAmount", rejectDefaultsCompanyAmount)
			);

			var sr = new SafeReader(dt.Rows[0]);

			_errorMPsNum = sr["ErrorMPsNum"];
			_loanOfferApprovalNum = sr["ApprovalNum"];
			_numOfDefaultAccounts = sr["NumOfDefaultAccounts"];
			_numOfCompanyDefaultAccounts = sr["NumOfDefaultCompanyAccounts"];
			
			_numOfLateAccounts = CountActiveAccounts();

			if (_isOffline)
			{
				dt = _db.ExecuteReader("GetHmrcAggregations", CommandSpecies.StoredProcedure,
										new QueryParameter("CustomerId", _customerId));
				sr = new SafeReader(dt.Rows[0]);
				_hasHmrc = sr["AnnualRevenues"] != -1 || sr["QuarterRevenues"] != -1;
				_hmrcAnnualRevenues = sr["AnnualRevenues"];
				_hmrcQuarterRevenues = sr["QuarterRevenues"];
			}

			_hasYodlee = _yodlee1YForRejection >= 0 || _yodlee3MForRejection >= 0;

			dt = _db.ExecuteReader(
				"GetPayPalAggregations",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", _customerId)
				);

			sr = new SafeReader(dt.Rows[0]);

			_payPalNumberOfStores = sr["PayPal_NumberOfStores"];
			_payPalTotalSumOfOrders3M = sr["PayPal_TotalSumOfOrders3M"];
			_payPalTotalSumOfOrders1Y = sr["PayPal_TotalSumOfOrders1Y"];
		}

		private int CountActiveAccounts()
		{
			// Fetch active accounts
			DataTable activeAccountsDataTable = _db.ExecuteReader(
				"GetCustomerActiveAccounts",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", _customerId)
			);

			int rejectLateLastMonth = CurrentValues.Instance.Reject_LateLastMonthsNum;
			string relevantStatuses = string.Empty;
			foreach (DataRow row in activeAccountsDataTable.Rows)
			{
				var accountSafeReader = new SafeReader(row);
				DateTime lastUpdateDate = accountSafeReader["LastUpdateDate"];
				int currentlyPointedStatus = 12;

				if (lastUpdateDate.AddMonths(rejectLateLastMonth) > DateTime.UtcNow) // If not then there is no relevant data
				{
					for (int i = 0; i < rejectLateLastMonth - 1; i++)
					{
						DateTime tmpDate = DateTime.UtcNow.AddMonths(-1 * i);
						if (tmpDate < lastUpdateDate || (tmpDate.Year == lastUpdateDate.Year && tmpDate.Month == lastUpdateDate.Month))
						{
							string fieldName = string.Format("StatusCode{0}", currentlyPointedStatus);
							currentlyPointedStatus--;
							string monthStatus = accountSafeReader[fieldName];
							relevantStatuses += monthStatus ?? string.Empty;
						}
					}
				}
			}

			int numOfRelevantAccounts = relevantStatuses.Length;
			for (int i = CurrentValues.Instance.RejectionLastValidLate + 1; i < 10; i++)
			{
				relevantStatuses = relevantStatuses.Replace(i.ToString(CultureInfo.InvariantCulture), "");
			}

			return numOfRelevantAccounts - relevantStatuses.Length;
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			try
			{
				Init();
				string rejectionExceptionReason;
				bool isRejectionException = IsException(out rejectionExceptionReason);

				int rejectionCompanyScore = CurrentValues.Instance.RejectionCompanyScore;

				// Consumer score < 500
				bool conditionValue = _maxExperianConsumerScore < _lowCreditScore;
				string conditionDescription = string.Format("Low personal score (_maxExperianConsumerScore < _lowCreditScore) [{0} < {1}]", _maxExperianConsumerScore, _lowCreditScore);
				_conditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue)
				{
					response.AutoRejectReason = "AutoReject: Low score. Condition not met:" + _maxExperianConsumerScore + " < " +
												_lowCreditScore;
				}
				// Business score < 10 (If business score exists)
				conditionValue = _maxCompanyScore > 0 && _maxCompanyScore < rejectionCompanyScore;
				conditionDescription = string.Format("Low business score (_maxCompanyScore > 0 && _maxCompanyScore < rejectionCompanyScore) [{0} > 0 && {1} < {2}]", _maxCompanyScore, _maxCompanyScore, rejectionCompanyScore);
				_conditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue && string.IsNullOrEmpty(response.AutoRejectReason))
				{
					response.AutoRejectReason = "AutoReject: Low company score. Condition not met:" + _maxCompanyScore + " < " +
												rejectionCompanyScore;
				}
				// Credit score < 800 AND at least 1 default of at least 300 in last 24 months
				conditionValue = _maxExperianConsumerScore < _rejectDefaultsCreditScore && _numOfDefaultAccounts >= _rejectDefaultsAccountsNum;
				conditionDescription = string.Format("Pretty low personal score and personal account defaults (_maxExperianConsumerScore < _rejectDefaultsCreditScore && _numOfDefaultAccounts >= _rejectDefaultsAccountsNum) [{0} < {1} && {2} >= {3}]", _maxExperianConsumerScore, _rejectDefaultsCreditScore, _numOfDefaultAccounts, _rejectDefaultsAccountsNum);
				_conditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue && string.IsNullOrEmpty(response.AutoRejectReason))
				{
					response.AutoRejectReason = "AutoReject: Score & DefaultAccountsNum. Condition not met:" +
												_maxExperianConsumerScore +
												" < " + _rejectDefaultsCreditScore + " AND " + _numOfDefaultAccounts + " >= " +
												_rejectDefaultsAccountsNum;
				}
				// Company score < 20 AND at least 1 default of at least 1000 in last 24 months
				conditionValue = _maxCompanyScore < _rejectDefaultsCompanyScore && _numOfCompanyDefaultAccounts >= _rejectDefaultsCompanyAccountsNum;
				conditionDescription = string.Format("Pretty low personal score and personal account defaults (_maxCompanyScore < _rejectDefaultsCompanyScore && _numOfCompanyDefaultAccounts >= _rejectDefaultsCompanyAccountsNum) [{0} < {1} && {2} >= {3}]", _maxCompanyScore, _rejectDefaultsCompanyScore, _numOfCompanyDefaultAccounts, _rejectDefaultsCompanyAccountsNum);
				_conditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue && string.IsNullOrEmpty(response.AutoRejectReason))
				{
					response.AutoRejectReason = "AutoReject: Score & DefaultAccountsNum. Condition not met:" +
												_rejectDefaultsCompanyScore +
												" < " + _numOfCompanyDefaultAccounts + " AND " + _numOfCompanyDefaultAccounts + " >= " +
												_rejectDefaultsCompanyAccountsNum;
				}
				// Late over 30 days in personal CAIS (should be configurable according to ExperianAccountStatuses) At least in 2 months (in one or more accounts) in last 3 months
				conditionValue = _numOfLateAccounts >= _rejectNumOfLateAccounts;
				conditionDescription = string.Format("Late in personal accounts (_numOfLateAccounts >= _rejectNumOfLateAccounts) [{0} >= {1}]", _numOfLateAccounts, _rejectNumOfLateAccounts);
				_conditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue && string.IsNullOrEmpty(response.AutoRejectReason))
				{
					response.AutoRejectReason = string.Format("AutoReject: Late CAIS accounts. Condition not met: {0} >= {1}",
					                                          _numOfLateAccounts, _rejectNumOfLateAccounts);
				}
				// max(Marketplace(Ecomm) or company )seniority < 300 days.
				conditionValue = Math.Max(_marketplaceSeniorityDays, _companySeniorityDays) < _rejectMinimalSeniority;
				conditionDescription = string.Format("Low company or MP seniority (Max(_marketplaceSeniorityDays, _companySeniorityDays) < _rejectMinimalSeniority) [Max({0}, {1}) < {2}]", _marketplaceSeniorityDays, _companySeniorityDays, _rejectMinimalSeniority);
				_conditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue && string.IsNullOrEmpty(response.AutoRejectReason))
				{
					response.AutoRejectReason = "AutoReject: Seniority. Condition not met: (max(mp " + _marketplaceSeniorityDays +
												", company " + _companySeniorityDays + ") < " +
												_rejectMinimalSeniority + ")";
				}
				// Customer status != enabled\fraud suspect
				conditionValue = !_customerStatusIsEnabled || _customerStatusIsWarning;
				conditionDescription = string.Format("Customer status (!_customerStatusIsEnabled || _customerStatusIsWarning) [!{0} || {1}]", _customerStatusIsEnabled, _customerStatusIsWarning);
				_conditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue && string.IsNullOrEmpty(response.AutoRejectReason))
				{
					response.AutoRejectReason = "AutoReject: Customer status. Condition not met:" + _customerStatusName;
				}
				// Sufficient annual or quarterly turnover (Pay pal is also being considered by itself)
				conditionValue = !_isOffline && 
					(_payPalNumberOfStores == 0 || _payPalTotalSumOfOrders3M < _lowTotalThreeMonthTurnover || _payPalTotalSumOfOrders1Y < _lowTotalAnnualTurnover)
					&& (_totalSumOfOrders3MTotalForRejection < _lowTotalThreeMonthTurnover || _totalSumOfOrders1YTotalForRejection < _lowTotalAnnualTurnover);
				conditionDescription = string.Format("Low online turnover (!_isOffline && (_payPalNumberOfStores == 0 || _payPalTotalSumOfOrders3M < _lowTotalThreeMonthTurnover || _payPalTotalSumOfOrders1Y < _lowTotalAnnualTurnover) && (_totalSumOfOrders3MTotalForRejection < _lowTotalThreeMonthTurnover || _totalSumOfOrders1YTotalForRejection < _lowTotalAnnualTurnover)) [!{0} && ({1} == 0 || {2} < {3} || {4} < {5}) && ({6} < {7} || {8} < {9})]", 
					_isOffline, _payPalNumberOfStores, _payPalTotalSumOfOrders3M, _lowTotalThreeMonthTurnover, _payPalTotalSumOfOrders1Y, _lowTotalAnnualTurnover, 
					_totalSumOfOrders3MTotalForRejection, _lowTotalThreeMonthTurnover, _totalSumOfOrders1YTotalForRejection, _lowTotalAnnualTurnover);
				_conditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue && string.IsNullOrEmpty(response.AutoRejectReason))
				{
					response.AutoRejectReason = "AutoReject: Online Totals. Condition not met: (" + _payPalNumberOfStores + " < 0 OR " +
												_payPalTotalSumOfOrders3M + " < " +
												_lowTotalThreeMonthTurnover + " OR " + _payPalTotalSumOfOrders1Y + " < " +
												_lowTotalAnnualTurnover + ") AND (" + _totalSumOfOrders3MTotalForRejection + " < " +
												_lowTotalThreeMonthTurnover + " OR " + _totalSumOfOrders1YTotalForRejection + " < " +
												_lowTotalAnnualTurnover + ")";
				}
				// Offline only (hmrc or bank) turnover: Have separate turnover configs. Annual - 30000, 3M - 5000
				conditionValue = _isOffline && (_hasHmrc || _hasYodlee) && (Math.Max((decimal)_yodlee1YForRejection, _hmrcAnnualRevenues) < _lowOfflineAnnualRevenue || Math.Max((decimal)_yodlee3MForRejection, _hmrcQuarterRevenues) < _lowOfflineQuarterRevenue);
				conditionDescription = string.Format("Low offline turnover (_isOffline && (_hasHmrc || _hasYodlee) && (Max(_yodlee1YForRejection, _hmrcAnnualRevenues) < _lowOfflineAnnualRevenue || Max(_yodlee3MForRejection, _hmrcQuarterRevenues) < _lowOfflineQuarterRevenue)) [{0} && ({1} || {2}) && (Max({3}, {4}) < {5} || Max({6}, {7}) < {8})]", _isOffline, _hasHmrc, _hasYodlee, (decimal)_yodlee1YForRejection, _hmrcAnnualRevenues, _lowOfflineAnnualRevenue, (decimal)_yodlee3MForRejection, _hmrcQuarterRevenues, _lowOfflineQuarterRevenue);
				_conditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue && string.IsNullOrEmpty(response.AutoRejectReason))
				{
					response.AutoRejectReason =
						string.Format(
							"AutoReject: Offline Revenues. Condition not met: max yodlee hmrc annual revenue ({0}, {1}) < {2} OR max yodlee hmrc quarter revenue ({3}, {4}) < {5}",
							_yodlee1YForRejection, _hmrcAnnualRevenues, _lowOfflineAnnualRevenue, _yodlee3MForRejection, _hmrcQuarterRevenues, _lowOfflineQuarterRevenue);
				}

				if (isRejectionException)
				{
					response.AutoRejectReason = rejectionExceptionReason;
					_log.Debug("Customer {0} was not auto rejected because {1}", _customerId, rejectionExceptionReason);
					return false;
				}

				if (string.IsNullOrEmpty(response.AutoRejectReason))
				{
					_log.Info("no auto rejetion for customer {0}", _customerId);
					return false;
				}

				_log.Info("customer {0} auto rejected", response.AutoRejectReason);

				response.CreditResult = _enableAutomaticRejection ? "Rejected" : "WaitingForDecision";
				response.UserStatus = "Rejected";
				response.SystemDecision = "Reject";
				FillFiguresForExplanationMail(response);
				return true;
			}
			catch (Exception e)
			{
				_log.Error("Exception during rejection:{0}", e);
				return false;
			}
		}

		public void FillFiguresForExplanationMail(AutoDecisionResponse response)
		{
			response.RejectionModel = new RejectionModel
				{
					NumOfDefaultAccounts = _numOfDefaultAccounts,
					PayPalNumberOfStores = _payPalNumberOfStores,
					PayPalTotalSumOfOrders1Y = _payPalTotalSumOfOrders1Y,
					PayPalTotalSumOfOrders3M = _payPalTotalSumOfOrders3M,
					Yodlee1Y = _yodlee1YForRejection,
					Yodlee3M = _yodlee3MForRejection,
					Hmrc1Y = _hmrcAnnualRevenues,
					Hmrc3M = _hmrcQuarterRevenues,
					HasYodlee = _hasYodlee,
					HasHmrc = _hasHmrc,
					CompanyScore = _maxCompanyScore,
					LateAccounts = _numOfLateAccounts
				};
		}

		public override string ToString()
		{
			return string.Format(
				@"Rejection input parameters for customer id {19}: \n 
					_autoRejectionExceptionAnualTurnover : {0} \n
					_rejectDefaultsCreditScore : {1} \n
					_rejectMinimalSeniority : {2} \n 
					_lowCreditScore : {3} \n 
					_rejectDefaultsAccountsNum : {4} \n
					_autoRejectionExceptionCreditScore : {5} \n
					_errorMPsNum : {6} \n 
					_loanOfferApprovalNum : {7} \n 
					_numOfDefaultAccounts : {8} \n
					_totalSumOfOrders1YTotalForRejection : {10} _totalSumOfOrders3MTotalForRejection : {11} \n 
					_marketplaceSeniorityDays : {14} \n 
					_enableAutomaticRejection : {15} \n 
					_lowTotalAnnualTurnover : {16} _lowTotalThreeMonthTurnover : {17} \n 
					_maxExperianConsumerScore : {18} _maxCompanyScore : {20} \n
					_customerStatusIsEnabled : {21} _customerStatusIsWarning : {22} _customerStatusName : {9} \n
					_isBrokerCustomer : {23} \n
					_isLimitedCompany : {24} \n
					_companySeniorityDays : {25} \n
					_isOffline : {26} \n
					_hasYodlee: {36} _yodlee1YForRejection : {12} _yodlee3MForRejection : {13} \n
					_hasHmrc: {37} _hmrcAnnualRevenues : {27} _hmrcQuarterRevenues : {28} \n
					_lowOfflineQuarterRevenue : {29} \n
					_lowOfflineAnnualRevenue : {30} \n
					_numOfLateAccounts : {31} \n
					_rejectNumOfLateAccounts : {32} \n
					_payPalNumberOfStores : {33} _payPalTotalSumOfOrders3M : {34} _payPalTotalSumOfOrders1Y : {35} \n
					_rejectDefaultsCompanyScore : {38} \n
					_numOfCompanyDefaultAccounts : {39} _rejectDefaultsCompanyAccountsNum : {40} \n",
				_autoRejectionExceptionAnualTurnover,
				_rejectDefaultsCreditScore,
				_rejectMinimalSeniority,
				_lowCreditScore,
				_rejectDefaultsAccountsNum,
				_autoRejectionExceptionCreditScore,
				_errorMPsNum,
				_loanOfferApprovalNum,
				_numOfDefaultAccounts,
				_customerStatusName,
				_totalSumOfOrders1YTotalForRejection,
				_totalSumOfOrders3MTotalForRejection,
				_yodlee1YForRejection,
				_yodlee3MForRejection,
				_marketplaceSeniorityDays,
				_enableAutomaticRejection,
				_lowTotalAnnualTurnover,
				_lowTotalThreeMonthTurnover,
				_maxExperianConsumerScore,
				_customerId,
				_maxCompanyScore,
				_customerStatusIsEnabled,
				_customerStatusIsWarning,
				_isBrokerCustomer,
				_isLimitedCompany,
				_companySeniorityDays,
				_isOffline,
				_hmrcAnnualRevenues,
				_hmrcQuarterRevenues,
				_lowOfflineQuarterRevenue,
				_lowOfflineAnnualRevenue,
				_numOfLateAccounts,
				_rejectNumOfLateAccounts,
				_payPalNumberOfStores,
				_payPalTotalSumOfOrders3M,
				_payPalTotalSumOfOrders1Y,
				_hasYodlee, 
				_hasHmrc,
				_rejectDefaultsCompanyScore,
				_numOfCompanyDefaultAccounts,
				_rejectDefaultsCompanyAccountsNum);
		} 
	}
}
