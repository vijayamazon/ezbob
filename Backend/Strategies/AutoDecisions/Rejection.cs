namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using System.Data;
	using Ezbob.Logger;
	
	public class Rejection
	{
		private int _autoRejectionExceptionAnualTurnover;
		private int _rejectDefaultsCreditScore;
		private int _rejectMinimalSeniority;
		private int _lowCreditScore;
		private int _rejectDefaultsAccountsNum;
		private int _autoRejectionExceptionCreditScore;
		private int _errorMPsNum;
		private int _loanOfferApprovalNum;
		private int _numOfDefaultAccounts;
		private int _numOfDefaultAccountsForCompany;
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

		public Rejection(int customerId,
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

		private bool IsException(out string reason)
		{
			reason = "AutoReject: Rejection exception";

			//1. Customers that have been approved at least once before (even if the latest decision was rejection) and in enabled status/fraud suspect
			if (_loanOfferApprovalNum > 0 && _customerStatusIsEnabled && !_customerStatusIsWarning)
			{
				reason = string.Format("{0} : {1}", reason, "Customers that have been approved at least once before");
				return true;
			}
			//2. Annual turnover above 250,000
			if (_totalSumOfOrders1YTotalForRejection > _autoRejectionExceptionAnualTurnover)
			{
				reason = string.Format("{0} : {1} {2} ({3})", reason, "Annual turnover above", _autoRejectionExceptionAnualTurnover, _totalSumOfOrders1YTotalForRejection);
				return true;
			}
			//3. Consumer score (max of applicant and directors) above 800
			if (_maxExperianConsumerScore > _autoRejectionExceptionCreditScore)
			{
				reason = string.Format("{0} : {1} {2} ({3})", reason, "Consumer score above", _autoRejectionExceptionCreditScore, _maxExperianConsumerScore);
				return true;
			}
			//4. Company score (max of company, parent companies) (default >= 40)
			int rejectionExceptionMaxCompanyScore = CurrentValues.Instance.RejectionExceptionMaxCompanyScore;
			if (_maxCompanyScore >= rejectionExceptionMaxCompanyScore)
			{
				reason = string.Format("{0} : {1} {2} ({3})", reason, "Company score above", rejectionExceptionMaxCompanyScore, _maxCompanyScore);
				return true;
			}
			//5. MP with error AND (consumer score > 500 OR company score > 10)
			int rejectionExceptionMaxConsumerScoreForMpError = CurrentValues.Instance.RejectionExceptionMaxConsumerScoreForMpError;
			int rejectionExceptionMaxCompanyScoreForMpError = CurrentValues.Instance.RejectionExceptionMaxCompanyScoreForMpError;
			if (_errorMPsNum > 0 && (_maxExperianConsumerScore > rejectionExceptionMaxConsumerScoreForMpError || _maxCompanyScore > rejectionExceptionMaxCompanyScoreForMpError))
			{
				reason = string.Format("{0} : {1} {2} {3}", reason, "MP with error and experian scores", _maxExperianConsumerScore, _maxCompanyScore);
				return true;
			}
			//TODO : unknown rule
			if ((decimal)_maxExperianConsumerScore == 0)
			{
				reason = string.Format("{0} : {1}", reason, "Experian consumer score is 0");
				return true;
			}
			//6. Customer via broker
			if (_isBrokerCustomer) // TODO: Currently rejections are disabled for broker customers - this logic is in contradiction to it
			{
				reason = string.Format("{0} : {1}", reason, "Customer via broker");
				return true;
			}

			reason = "No rejection exception";
			_log.Info("customerId {0} {1}", _customerId, reason);
			return false;
		}

		private void Init()
		{
			DataTable dt = _db.ExecuteReader("GetRejectionConfigs", CommandSpecies.StoredProcedure);
			var sr = new SafeReader(dt.Rows[0]);

			_autoRejectionExceptionAnualTurnover = sr["AutoRejectionException_AnualTurnover"];
			_rejectDefaultsCreditScore = sr["Reject_Defaults_CreditScore"];
			_rejectMinimalSeniority = sr["Reject_Minimal_Seniority"];
			_lowCreditScore = sr["LowCreditScore"];
			_rejectDefaultsAccountsNum = sr["Reject_Defaults_AccountsNum"];
			_autoRejectionExceptionCreditScore = sr["AutoRejectionException_CreditScore"];
			int rejectDefaultsMonths = sr["Reject_Defaults_MonthsNum"];
			int rejectDefaultsAmount = sr["Reject_Defaults_Amount"];
			int rejectByCompanyDefaultsMonths = CurrentValues.Instance.RejectByCompany_Defaults_MonthsNum;
			int rejectByCompanyDefaultsAmount = CurrentValues.Instance.RejectByCompany_Defaults_Amount;
			_lowTotalAnnualTurnover = sr["LowTotalAnnualTurnover"];
			_lowTotalThreeMonthTurnover = sr["LowTotalThreeMonthTurnover"];
			_lowOfflineAnnualRevenue = sr["Reject_LowOfflineAnnualRevenue"];
			_lowOfflineQuarterRevenue = sr["Reject_LowOfflineQuarterRevenue"];

			int rejectLateLastMonth = CurrentValues.Instance.Reject_LateLastMonthsNum;
			_rejectNumOfLateAccounts = CurrentValues.Instance.Reject_NumOfLateAccounts;

			dt = _db.ExecuteReader(
				"GetCustomerRejectionData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", _customerId),
				new QueryParameter("Reject_Defaults_Months", rejectDefaultsMonths),
				new QueryParameter("Reject_Defaults_Amount", rejectDefaultsAmount),
				new QueryParameter("RejectByCompany_Defaults_Months", rejectByCompanyDefaultsMonths),
				new QueryParameter("RejectByCompany_Defaults_Amount", rejectByCompanyDefaultsAmount),
				new QueryParameter("Reject_Late_Last_Months", rejectLateLastMonth)
			);

			sr = new SafeReader(dt.Rows[0]);

			_errorMPsNum = sr["ErrorMPsNum"];
			_loanOfferApprovalNum = sr["ApprovalNum"];
			_numOfDefaultAccounts = sr["NumOfDefaultAccounts"];
			_numOfDefaultAccountsForCompany = sr["NumOfDefaultAccountsForCompany"]; // TODO: this is wrong, should have logic elsewhere
			_numOfLateAccounts = sr["NumOfLateAccounts"];
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

		public bool MakeDecision(AutoDecisionResponse response)
		{
			try
			{
				Init();
				string rejectionExceptionReason;
				if (IsException(out rejectionExceptionReason))
				{
					response.AutoRejectReason = rejectionExceptionReason;
					_log.Debug("Customer {0} was not auto rejected because {1}", _customerId, rejectionExceptionReason);
					return false;
				}

				int rejectByCompanyNumOfDefaultAccounts = CurrentValues.Instance.RejectByCompanyNumOfDefaultAccounts;
				int rejectByCompanyDefaultsScore = CurrentValues.Instance.RejectByCompanyDefaultsScore;
				int rejectionCompanyScore = CurrentValues.Instance.RejectionCompanyScore;

				//1. Consumer score < 500
				if (_maxExperianConsumerScore < _lowCreditScore)
				{
					response.AutoRejectReason = "AutoReject: Low score. Condition not met:" + _maxExperianConsumerScore + " < " +
												_lowCreditScore;
				}
				//2. Business score < 10 (If business score exists)
				else if (_maxCompanyScore > 0 && _maxCompanyScore < rejectionCompanyScore)
				{
					response.AutoRejectReason = "AutoReject: Low company score. Condition not met:" + _maxCompanyScore + " < " +
												rejectionCompanyScore;
				}
				//3. Credit score < 800 AND at least 1 default of at least 300 in last 24 months
				else if (_maxExperianConsumerScore < _rejectDefaultsCreditScore &&
					_numOfDefaultAccounts >= _rejectDefaultsAccountsNum)
				{
					response.AutoRejectReason = "AutoReject: Score & DefaultAccountsNum. Condition not met:" +
												_maxExperianConsumerScore +
												" < " + _rejectDefaultsCreditScore + " AND " + _numOfDefaultAccounts + " >= " +
												_rejectDefaultsAccountsNum;
				}
				//4. Business score exists and < 20 AND at least 1 company default of at least 1000 in last 24 months and company is limited
				else if (_maxCompanyScore > 0 && _maxCompanyScore < rejectByCompanyDefaultsScore &&
				         _numOfDefaultAccountsForCompany >= rejectByCompanyNumOfDefaultAccounts && _isLimitedCompany)
				{
					response.AutoRejectReason = "AutoReject: Limited company defaults. Condition not met:" +
					                            _maxCompanyScore + "<" + rejectByCompanyDefaultsScore + " AND " +
					                            _numOfDefaultAccountsForCompany +
					                            " >= " + rejectByCompanyNumOfDefaultAccounts + " AND is limited company";
				}
				//5. Late over 30 days in personal CAIS (should be configurable according to ExperianAccountStatuses) At least in 2 accounts in last 3 months
				else if(_numOfLateAccounts >= _rejectNumOfLateAccounts)
				{
					response.AutoRejectReason = string.Format("AutoReject: Late CAIS accounts. Condition not met: {0} >= {1}",
					                                          _numOfLateAccounts, _rejectNumOfLateAccounts);
				}
				//6. max(Marketplace(Ecomm) or company )seniority < 300 days.
				else if (Math.Max(_marketplaceSeniorityDays, _companySeniorityDays) < _rejectMinimalSeniority)
				{
					response.AutoRejectReason = "AutoReject: Seniority. Condition not met: (max(mp " + _marketplaceSeniorityDays +
												", company " + _companySeniorityDays + ") < " +
												_rejectMinimalSeniority + ")";
				}
				//7. Customer status != enabled\fraud suspect
				else if (!_customerStatusIsEnabled || _customerStatusIsWarning)
				{
					response.AutoRejectReason = "AutoReject: Customer status. Condition not met:" + _customerStatusName;

				}

				// TODO: the 2 next conditions are in the previous implementation but are not mentioned in the new story - should they be removed?
				// TODO: should this next condition be removed (was it replaced by condition #1 (second paragraph) in story?)
				else if (!_isOffline && 
					(_payPalNumberOfStores == 0 || _payPalTotalSumOfOrders3M < _lowTotalThreeMonthTurnover || _payPalTotalSumOfOrders1Y < _lowTotalAnnualTurnover)
					&& (_totalSumOfOrders3MTotalForRejection < _lowTotalThreeMonthTurnover || _totalSumOfOrders1YTotalForRejection < _lowTotalAnnualTurnover))
				{
					response.AutoRejectReason = "AutoReject: Online Totals. Condition not met: (" + _payPalNumberOfStores + " < 0 OR " +
												_payPalTotalSumOfOrders3M + " < " +
												_lowTotalThreeMonthTurnover + " OR " + _payPalTotalSumOfOrders1Y + " < " +
												_lowTotalAnnualTurnover + ") AND (" + _totalSumOfOrders3MTotalForRejection + " < " +
												_lowTotalThreeMonthTurnover + " OR " + _totalSumOfOrders1YTotalForRejection + " < " +
												_lowTotalAnnualTurnover + ")";
				}
				
				//Offline only (hmrc or bank) 1. Have separate turnover configs. Annual - 30000, 3M - 5000
				else if(_isOffline && (_hasHmrc || _hasYodlee) && 
					(Math.Max((decimal)_yodlee1YForRejection, _hmrcAnnualRevenues) < _lowOfflineAnnualRevenue || 
					 Math.Max((decimal)_yodlee3MForRejection, _hmrcQuarterRevenues) < _lowOfflineQuarterRevenue))
				{
					response.AutoRejectReason =
						string.Format(
							"AutoReject: Offline Revenues. Condition not met: max yodlee hmrc annual revenue ({0}, {1}) < {2} OR max yodlee hmrc quarter revenue ({3}, {4}) < {5}",
							_yodlee1YForRejection, _hmrcAnnualRevenues, _lowOfflineAnnualRevenue, _yodlee3MForRejection, _hmrcQuarterRevenues, _lowOfflineQuarterRevenue);
				}
				else
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
					_numOfDefaultAccounts : {8} _numOfDefaultAccountsForCompany : {9} \n
					_totalSumOfOrders1YTotalForRejection : {10} _totalSumOfOrders3MTotalForRejection : {11} \n 
					_marketplaceSeniorityDays : {14} \n 
					_enableAutomaticRejection : {15} \n 
					_lowTotalAnnualTurnover : {16} _lowTotalThreeMonthTurnover : {17} \n 
					_maxExperianConsumerScore : {18} _maxCompanyScore : {20} \n
					_customerStatusIsEnabled : {21} _customerStatusIsWarning : {22} _customerStatusName : {38} \n
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
					_payPalNumberOfStores : {33} _payPalTotalSumOfOrders3M : {34} _payPalTotalSumOfOrders1Y : {35} \n",
				_autoRejectionExceptionAnualTurnover,
				_rejectDefaultsCreditScore,
				_rejectMinimalSeniority,
				_lowCreditScore,
				_rejectDefaultsAccountsNum,
				_autoRejectionExceptionCreditScore,
				_errorMPsNum,
				_loanOfferApprovalNum,
				_numOfDefaultAccounts,
				_numOfDefaultAccountsForCompany,
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
				_hasYodlee, _hasHmrc,
				_customerStatusName);
		} 
	}
}
