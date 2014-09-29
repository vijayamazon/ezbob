namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using ConfigManager;
	using Experian;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
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
		private bool hasCompanyFiles;

		public Rejection(
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
		}

		private bool IsException(out string reason, List<AutoDecisionCondition> rejectionConditions)
		{
			reason = "AutoReject: Rejection exception";

			// Customers that have been approved at least once before (even if the latest decision was rejection) and in enabled status/fraud suspect
			bool conditionValue = _loanOfferApprovalNum > 0 && _customerStatusIsEnabled && !_customerStatusIsWarning;
			string conditionDescription = string.Format("Exception - Has past approval and customer status is enabled and not warning (_loanOfferApprovalNum > 0 && _customerStatusIsEnabled && !_customerStatusIsWarning) [{0} > 0 && {1} && !{2}]", _loanOfferApprovalNum, _customerStatusIsEnabled, _customerStatusIsWarning);
			rejectionConditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
			if (conditionValue)
			{
				reason = string.Format("{0} : {1}", reason, "Customers that have been approved at least once before");
			}
			// Annual turnover above 250,000
			conditionValue = _totalSumOfOrders1YTotalForRejection > _autoRejectionExceptionAnualTurnover;
			conditionDescription = string.Format("Exception - High annual turnover (_totalSumOfOrders1YTotalForRejection > _autoRejectionExceptionAnualTurnover) [{0} > {1}]", _totalSumOfOrders1YTotalForRejection, _autoRejectionExceptionAnualTurnover);
			rejectionConditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
			if (conditionValue)
			{
				reason = string.Format("{0} : {1} {2} ({3})", reason, "Annual turnover above", _autoRejectionExceptionAnualTurnover, _totalSumOfOrders1YTotalForRejection);
			}
			// Consumer score (max of applicant and directors) above 800
			conditionValue = _maxExperianConsumerScore > _autoRejectionExceptionCreditScore;
			conditionDescription = string.Format("Exception - High max personal score (_maxExperianConsumerScore > _autoRejectionExceptionCreditScore) [{0} > {1}]", _maxExperianConsumerScore, _autoRejectionExceptionCreditScore);
			rejectionConditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
			if (conditionValue)
			{
				reason = string.Format("{0} : {1} {2} ({3})", reason, "Consumer score above", _autoRejectionExceptionCreditScore, _maxExperianConsumerScore);
			}
			// Company score (max of company, parent companies) (default >= 40)
			int rejectionExceptionMaxCompanyScore = CurrentValues.Instance.RejectionExceptionMaxCompanyScore;
			conditionValue = _maxCompanyScore >= rejectionExceptionMaxCompanyScore;
			conditionDescription = string.Format("Exception - High max company score (_maxCompanyScore >= rejectionExceptionMaxCompanyScore) [{0} >= {1}]", _maxCompanyScore, rejectionExceptionMaxCompanyScore);
			rejectionConditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
			if (conditionValue)
			{
				reason = string.Format("{0} : {1} {2} ({3})", reason, "Company score above", rejectionExceptionMaxCompanyScore, _maxCompanyScore);
			}
			// MP with error AND (consumer score > 500 OR company score > 10)
			int rejectionExceptionMaxConsumerScoreForMpError = CurrentValues.Instance.RejectionExceptionMaxConsumerScoreForMpError;
			int rejectionExceptionMaxCompanyScoreForMpError = CurrentValues.Instance.RejectionExceptionMaxCompanyScoreForMpError;
			conditionValue = _errorMPsNum > 0 && (_maxExperianConsumerScore > rejectionExceptionMaxConsumerScoreForMpError || _maxCompanyScore > rejectionExceptionMaxCompanyScoreForMpError);
			conditionDescription = string.Format("Exception - MP with error and high enough experian score (_errorMPsNum > 0 && (_maxExperianConsumerScore > rejectionExceptionMaxConsumerScoreForMpError || _maxCompanyScore > rejectionExceptionMaxCompanyScoreForMpError)) [{0} > 0 && ({1} > {2} || {3} > {4})]", _errorMPsNum, _maxExperianConsumerScore, rejectionExceptionMaxConsumerScoreForMpError, _maxCompanyScore, rejectionExceptionMaxCompanyScoreForMpError);
			rejectionConditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
			if (conditionValue)
			{
				reason = string.Format("{0} : {1} {2} {3}", reason, "MP with error and experian scores", _maxExperianConsumerScore, _maxCompanyScore);
			}
			// Customer via broker
			conditionValue = _isBrokerCustomer;
			conditionDescription = string.Format("Exception - Customer via broker (_isBrokerCustomer) [{0}]", _isBrokerCustomer);
			rejectionConditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
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

			SafeReader sr = _db.GetFirst(
				"GetCustomerRejectionData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", _customerId),
				new QueryParameter("Reject_Defaults_Months", rejectDefaultsMonths),
				new QueryParameter("Reject_Defaults_Amount", rejectDefaultsAmount),
				new QueryParameter("Reject_Defaults_CompanyMonths", rejectDefaultsCompanyMonths),
				new QueryParameter("Reject_Defaults_CompanyAmount", rejectDefaultsCompanyAmount)
			);

			_errorMPsNum = sr["ErrorMPsNum"];
			_loanOfferApprovalNum = sr["ApprovalNum"];
			_numOfDefaultAccounts = sr["NumOfDefaultAccounts"];
			_numOfCompanyDefaultAccounts = sr["NumOfDefaultCompanyAccounts"];
			hasCompanyFiles = sr["HasCompanyFiles"];
			_numOfLateAccounts = CountLateAccounts();

			if (_isOffline)
			{
				sr = _db.GetFirst("GetHmrcAggregations", CommandSpecies.StoredProcedure,
										new QueryParameter("CustomerId", _customerId));

				_hasHmrc = sr["AnnualRevenues"] != -1 || sr["QuarterRevenues"] != -1;
				_hmrcAnnualRevenues = sr["AnnualRevenues"];
				_hmrcQuarterRevenues = sr["QuarterRevenues"];
			}

			_hasYodlee = _yodlee1YForRejection >= 0 || _yodlee3MForRejection >= 0;

			sr = _db.GetFirst(
				"GetPayPalAggregations",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", _customerId)
				);

			_payPalNumberOfStores = sr["PayPal_NumberOfStores"];
			_payPalTotalSumOfOrders3M = sr["PayPal_TotalSumOfOrders3M"];
			_payPalTotalSumOfOrders1Y = sr["PayPal_TotalSumOfOrders1Y"];
		}

		private int CountLateAccounts()
		{
			int numOfLateAccounts = 0;
			var data = new LoadExperianConsumerData(_customerId, null, null, _db, _log);
			data.Execute();
			if (data.Result.ServiceLogId != null && data.Result.Cais.Any())
			{
				int rejectLateLastMonth = CurrentValues.Instance.Reject_LateLastMonthsNum;
				int rejectValidLate = CurrentValues.Instance.RejectionLastValidLate;

				foreach (var cais in data.Result.Cais)
				{
					DateTime lastUpdateDate = cais.LastUpdatedDate.HasValue ? cais.LastUpdatedDate.Value : new DateTime(1900,0,0);
					var days = (lastUpdateDate - DateTime.UtcNow.AddMonths(-rejectLateLastMonth)).TotalDays;
					int numOfRelevantStatuses = (int)Math.Ceiling(days / 30.0);
					if (numOfRelevantStatuses > 0) // If not then there is no relevant data
					{
						var relevantStatuses = cais.AccountStatusCodes.Substring(cais.AccountStatusCodes.Length - numOfRelevantStatuses,
						                                                         numOfRelevantStatuses).ToArray();
						foreach (var status in relevantStatuses)
						{
							int nStatus;
							int.TryParse(status.ToString(CultureInfo.InvariantCulture), out nStatus);
							if (nStatus > rejectValidLate && nStatus<8)
							{
								numOfLateAccounts++;
								break;
							}
						} 
					}
				}
			}
			return numOfLateAccounts;
		}

		public bool MakeDecision(AutoDecisionRejectionResponse response)
		{
			try
			{
				Init();
				string rejectionExceptionReason;
				bool isRejectionException = IsException(out rejectionExceptionReason, response.RejectionConditions);

				int rejectionCompanyScore = CurrentValues.Instance.RejectionCompanyScore;

				// 0 < Consumer score < 500
				bool conditionValue = 0 < _maxExperianConsumerScore && _maxExperianConsumerScore < _lowCreditScore;
				string conditionDescription = string.Format("Low personal score (0 < _maxExperianConsumerScore && _maxExperianConsumerScore < _lowCreditScore) [0 < {0} && {0} < {1}]", _maxExperianConsumerScore, _lowCreditScore);
				response.RejectionConditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue)
				{
					response.AutoRejectReason = "AutoReject: Low score. Condition met: 0 < " + _maxExperianConsumerScore + " AND " + _maxExperianConsumerScore + " < " +
												_lowCreditScore;
				}
				// 0 < Business score < 10 (If business score exists)
				conditionValue = _maxCompanyScore > 0 && _maxCompanyScore < rejectionCompanyScore;
				conditionDescription = string.Format("Low business score (_maxCompanyScore > 0 && _maxCompanyScore < rejectionCompanyScore) [{0} > 0 && {1} < {2}]", _maxCompanyScore, _maxCompanyScore, rejectionCompanyScore);
				response.RejectionConditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue && string.IsNullOrEmpty(response.AutoRejectReason))
				{
					response.AutoRejectReason = "AutoReject: Low company score. Condition met: 0 < " + _maxCompanyScore + " < " +
												rejectionCompanyScore;
				}
				// 0 < Credit score < 800 AND at least 1 default of at least 300 in last 24 months
				conditionValue = 0 < _maxExperianConsumerScore && _maxExperianConsumerScore < _rejectDefaultsCreditScore && _numOfDefaultAccounts >= _rejectDefaultsAccountsNum;
				conditionDescription = string.Format("Pretty low personal score and personal account defaults (0 < _maxExperianConsumerScore && _maxExperianConsumerScore < _rejectDefaultsCreditScore && _numOfDefaultAccounts >= _rejectDefaultsAccountsNum) [0 < {0} && {0} < {1} && {2} >= {3}]", _maxExperianConsumerScore, _rejectDefaultsCreditScore, _numOfDefaultAccounts, _rejectDefaultsAccountsNum);
				response.RejectionConditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue && string.IsNullOrEmpty(response.AutoRejectReason))
				{
					response.AutoRejectReason = "AutoReject: Score & DefaultAccountsNum. Condition met: 0 < " +
												_maxExperianConsumerScore + " AND " +
												_maxExperianConsumerScore + " < " + _rejectDefaultsCreditScore + " AND " + _numOfDefaultAccounts + " >= " +
												_rejectDefaultsAccountsNum;
				}
				// 0 < Company score < 20 AND at least 1 default of at least 1000 in last 24 months
				conditionValue = 0 < _maxCompanyScore && _maxCompanyScore < _rejectDefaultsCompanyScore && _numOfCompanyDefaultAccounts >= _rejectDefaultsCompanyAccountsNum;
				conditionDescription = string.Format("Pretty low personal score and personal account defaults (0 < _maxCompanyScore && _maxCompanyScore < _rejectDefaultsCompanyScore && _numOfCompanyDefaultAccounts >= _rejectDefaultsCompanyAccountsNum) [0 < {0} && {0} < {1} && {2} >= {3}]", _maxCompanyScore, _rejectDefaultsCompanyScore, _numOfCompanyDefaultAccounts, _rejectDefaultsCompanyAccountsNum);
				response.RejectionConditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue && string.IsNullOrEmpty(response.AutoRejectReason))
				{
					response.AutoRejectReason = "AutoReject: Score & DefaultAccountsNum. Condition met: 0 < " + _maxCompanyScore + " AND " +
												_maxCompanyScore +
												" < " + _rejectDefaultsCompanyScore + " AND " + _numOfCompanyDefaultAccounts + " >= " +
												_rejectDefaultsCompanyAccountsNum;
				}
				// Late over 30 days in personal CAIS (should be configurable according to ExperianAccountStatuses) At least in 2 months (in one or more accounts) in last 3 months
				conditionValue = _numOfLateAccounts >= _rejectNumOfLateAccounts;
				conditionDescription = string.Format("Late in personal accounts (_numOfLateAccounts >= _rejectNumOfLateAccounts) [{0} >= {1}]", _numOfLateAccounts, _rejectNumOfLateAccounts);
				response.RejectionConditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue && string.IsNullOrEmpty(response.AutoRejectReason))
				{
					response.AutoRejectReason = string.Format("AutoReject: Late CAIS accounts. Condition met: {0} >= {1}",
					                                          _numOfLateAccounts, _rejectNumOfLateAccounts);
				}
				// (0 < max(Marketplace(Ecomm) or company )seniority < 300 days) && !hasCompanyFiles
				double maxSeniority = Math.Max(_marketplaceSeniorityDays, _companySeniorityDays);
				conditionValue = (0 < maxSeniority && maxSeniority < _rejectMinimalSeniority) && !hasCompanyFiles;
				conditionDescription = string.Format("Low company and MP seniority ((0 < Max(_marketplaceSeniorityDays, _companySeniorityDays) && Max(_marketplaceSeniorityDays, _companySeniorityDays) < _rejectMinimalSeniority) && !hasCompanyFiles) [(0 < Max({0}, {1}) && Max({0}, {1}) < {2}) && !{3}]", _marketplaceSeniorityDays, _companySeniorityDays, _rejectMinimalSeniority, hasCompanyFiles);
				response.RejectionConditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue && string.IsNullOrEmpty(response.AutoRejectReason))
				{
					response.AutoRejectReason = "AutoReject: Seniority. Condition met: (0 < (max(mp " + _marketplaceSeniorityDays +
												", company " + _companySeniorityDays + ") AND (max(mp " + _marketplaceSeniorityDays +
												", company " + _companySeniorityDays + ") < " +
												_rejectMinimalSeniority + ")) AND NOT " + hasCompanyFiles;
				}
				// Customer status != enabled\fraud suspect
				conditionValue = !_customerStatusIsEnabled || _customerStatusIsWarning;
				conditionDescription = string.Format("Customer status (!_customerStatusIsEnabled || _customerStatusIsWarning) [!{0} || {1}]", _customerStatusIsEnabled, _customerStatusIsWarning);
				response.RejectionConditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue && string.IsNullOrEmpty(response.AutoRejectReason))
				{
					response.AutoRejectReason = "AutoReject: Customer status. Condition met:" + _customerStatusName;
				}
				// !hasCompanyFiles && !_isOffline && Sufficient annual or quarterly turnover (Pay pal is also being considered by itself)
				conditionValue = !hasCompanyFiles && !_isOffline && 
					(_payPalNumberOfStores == 0 || _payPalTotalSumOfOrders3M < _lowTotalThreeMonthTurnover || _payPalTotalSumOfOrders1Y < _lowTotalAnnualTurnover)
					&& (_totalSumOfOrders3MTotalForRejection < _lowTotalThreeMonthTurnover || _totalSumOfOrders1YTotalForRejection < _lowTotalAnnualTurnover);
				conditionDescription = string.Format("Low online turnover (!hasCompanyFiles && !_isOffline && (_payPalNumberOfStores == 0 || _payPalTotalSumOfOrders3M < _lowTotalThreeMonthTurnover || _payPalTotalSumOfOrders1Y < _lowTotalAnnualTurnover) && (_totalSumOfOrders3MTotalForRejection < _lowTotalThreeMonthTurnover || _totalSumOfOrders1YTotalForRejection < _lowTotalAnnualTurnover)) [!{10} && !{0} && ({1} == 0 || {2} < {3} || {4} < {5}) && ({6} < {7} || {8} < {9})]", 
					_isOffline, _payPalNumberOfStores, _payPalTotalSumOfOrders3M, _lowTotalThreeMonthTurnover, _payPalTotalSumOfOrders1Y, _lowTotalAnnualTurnover, 
					_totalSumOfOrders3MTotalForRejection, _lowTotalThreeMonthTurnover, _totalSumOfOrders1YTotalForRejection, _lowTotalAnnualTurnover, hasCompanyFiles);
				response.RejectionConditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue && string.IsNullOrEmpty(response.AutoRejectReason))
				{
					response.AutoRejectReason = "AutoReject: Online Totals. Condition met: NOT " + hasCompanyFiles + " AND NOT " + _isOffline + " AND (" + _payPalNumberOfStores + " < 0 OR " +
												_payPalTotalSumOfOrders3M + " < " +
												_lowTotalThreeMonthTurnover + " OR " + _payPalTotalSumOfOrders1Y + " < " +
												_lowTotalAnnualTurnover + ") AND (" + _totalSumOfOrders3MTotalForRejection + " < " +
												_lowTotalThreeMonthTurnover + " OR " + _totalSumOfOrders1YTotalForRejection + " < " +
												_lowTotalAnnualTurnover + ")";
				}
				// !hasCompanyFiles && Offline only (hmrc or bank) turnover: Have separate turnover configs. Annual - 30000, 3M - 5000
				conditionValue = !hasCompanyFiles && _isOffline && (_hasHmrc || _hasYodlee) && (Math.Max((decimal)_yodlee1YForRejection, _hmrcAnnualRevenues) < _lowOfflineAnnualRevenue || Math.Max((decimal)_yodlee3MForRejection, _hmrcQuarterRevenues) < _lowOfflineQuarterRevenue);
				conditionDescription = string.Format("Low offline turnover (!hasCompanyFiles && _isOffline && (_hasHmrc || _hasYodlee) && (Max(_yodlee1YForRejection, _hmrcAnnualRevenues) < _lowOfflineAnnualRevenue || Max(_yodlee3MForRejection, _hmrcQuarterRevenues) < _lowOfflineQuarterRevenue)) [!{9} && {0} && ({1} || {2}) && (Max({3}, {4}) < {5} || Max({6}, {7}) < {8})]", _isOffline, _hasHmrc, _hasYodlee, (decimal)_yodlee1YForRejection, _hmrcAnnualRevenues, _lowOfflineAnnualRevenue, (decimal)_yodlee3MForRejection, _hmrcQuarterRevenues, _lowOfflineQuarterRevenue, hasCompanyFiles);
				response.RejectionConditions.Add(new AutoDecisionCondition { DecisionName = "Rejection", Description = conditionDescription, Satisfied = conditionValue });
				if (conditionValue && string.IsNullOrEmpty(response.AutoRejectReason))
				{
					response.AutoRejectReason =
						string.Format(
							"AutoReject: Offline Revenues. Condition met: [NOT {0} AND {1} AND ({2} OR {3}) AND (max yodlee hmrc annual revenue ({4}, {5}) < {6} OR max yodlee hmrc quarter revenue ({7}, {8}) < {9})]",
							hasCompanyFiles, _isOffline, _hasHmrc, _hasYodlee, _yodlee1YForRejection, _hmrcAnnualRevenues, _lowOfflineAnnualRevenue, _yodlee3MForRejection, _hmrcQuarterRevenues, _lowOfflineQuarterRevenue);
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
				response.DecidedToReject = true;
				FillFiguresForExplanationMail(response);
				return true;
			}
			catch (Exception e)
			{
				_log.Error("Exception during rejection:{0}", e);
				return false;
			}
		}

		public void FillFiguresForExplanationMail(AutoDecisionRejectionResponse response)
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
