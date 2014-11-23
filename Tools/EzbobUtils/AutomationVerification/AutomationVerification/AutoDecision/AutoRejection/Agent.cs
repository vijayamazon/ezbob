﻿namespace AutomationCalculator.AutoDecision.AutoRejection
{
	using System;
	using AutoApproval;
	using ProcessHistory.AutoRejection;
	using ProcessHistory.Trails;
	using Common;
	using ProcessHistory;
	using ProcessHistory.Common;
	using Ezbob.Database;
	using Ezbob.Logger;

	/// <summary>
	/// Rejection Agent is will determine weather client should be auto rejected or not
	/// </summary>
	public class RejectionAgent
	{
		private readonly DbHelper _dbHelper;
		private readonly MarketPlacesHelper _mpHelper;

		#region public

		#region constructor

		/// <summary>
		/// Constructor get db, log customer id and rejection configuration variables
		/// </summary>
		public RejectionAgent(AConnection oDB, ASafeLog oLog, int nCustomerID, RejectionConfigs configs = null)
		{
			_customerId = nCustomerID;
			_isAutoRejected = false;
			m_oLog = oLog;
			m_oDB = oDB;
			_mpHelper = new MarketPlacesHelper(m_oLog);
			_dbHelper = new DbHelper(oDB, oLog);
			if (configs == null) {
				configs = _dbHelper.GetRejectionConfigs();
			}
			_configs = configs;
			Trail = new RejectionTrail(nCustomerID, oLog);
		} // constructor

		#endregion constructor

		/// <summary>
		/// Retrieves customer's rejection input data
		/// </summary>
		/// <param name="dataAsOf">optional parameter to retrieve historical data for rejection</param>
		/// <returns></returns>
		public RejectionInputData GetRejectionInputData(DateTime? dataAsOf) {
			DateTime now = dataAsOf.HasValue ? dataAsOf.Value : DateTime.UtcNow;
			var model = new RejectionInputData();
			var dbData = _dbHelper.GetRejectionData(_customerId);
			
			var originationTime = new OriginationTime(m_oLog);
			m_oDB.ForEachRowSafe(originationTime.Process, "LoadCustomerMarketplaceOriginationTimes",
			                     CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", _customerId));
			originationTime.FromExperian(dbData.IncorporationDate);

			var days = originationTime.Since.HasValue ? (now - originationTime.Since.Value).TotalDays : 0;

			_mpHelper.GetMarketPlacesSeniority(_dbHelper.GetCustomerMarketPlaces(_customerId));

			var data = new RejectionInputData {
				CustomerStatus = dbData.CustomerStatus,
				ConsumerScore = dbData.ExperianScore,
				BusinessScore = dbData.CompanyScore,
				WasApproved = dbData.WasApproved,
				NumOfDefaultConsumerAccounts = dbData.DefaultAccountsNum,
				NumOfDefaultBusinessAccounts = dbData.DefaultCompanyAccountsNum,
				DefaultAmountInConsumerAccounts = dbData.DefaultAccountAmount,
				DefaultAmountInBusinessAccounts = dbData.DefaultCompanyAccountAmount,
				HasMpError = dbData.HasErrorMp,
				HasCompanyFiles = dbData.HasCompanyFiles,
				BusinessSeniorityDays = (int)days,
				AnnualTurnover = 0, //TODO
				QuarterTurnover = 0, //TODO
				NumOfLateConsumerAccounts = 0, //TODO
				ConsumerLateDays = 0 //TODO
			};
		
			model.Init(now, data, _configs);
			return model;
		}

		#region method MakeDecision

		/// <summary>
		/// Main logic flow function to determine weather to auto reject the customer or not 
		/// </summary>
		/// <param name="data">rejection input data</param>
		public void MakeDecision(RejectionInputData data)
		{
			m_oLog.Debug("Checking if auto reject should take place for customer {0}...", _customerId);
			try {
				CheckRejectionExceptions();
				Trail.LockDecision();
				CheckRejections();
			}
			catch (Exception e)
			{
				m_oLog.Error(e, "Exception during auto rejection.");
				StepNoDecision<ExceptionThrown>().Init(e);
			} // try

			if (Trail.HasDecided) {
				_isAutoRejected = true;
			}

			m_oLog.Debug(
				"Checking if auto rejection should take place for customer {0} complete; {1}\n{2}",
				_customerId,
				Trail,
				_isAutoRejected ? "Auto rejected" : "Not auto rejected"
			);
		}

		#endregion method MakeDecision

		public RejectionTrail Trail { get; private set; }

		#endregion public

		#region private

		#region steps

		#region rejection exception checks

		/// <summary>
		/// Rejection exception steps - if one of them determins no reject - the client won't be auto rejected
		/// </summary>
		private void CheckRejectionExceptions()
		{
			CheckWasApproved();
			CheckHighAnnualTurnover();
			CheckBrokerClient();
			CheckHighConsumerScore();
			CheckHighBusinessScore();
			CheckMpError();
		}

		private void CheckWasApproved()
		{
			if (Trail.MyInputData.WasApproved)
			{
				StepNoReject<WasApproved>(true).Init(Trail.MyInputData.WasApproved);
			}
			else
			{
				StepNoDecision<WasApproved>().Init(Trail.MyInputData.WasApproved);
			}
		}

		private void CheckHighAnnualTurnover()
		{
			if (Trail.MyInputData.AnnualTurnover > Trail.MyInputData.AutoRejectionException_AnualTurnover)
			{
				StepNoReject<AnnualTurnoverException>(true).Init(Trail.MyInputData.AnnualTurnover, Trail.MyInputData.AutoRejectionException_AnualTurnover);
			}
			else
			{
				StepNoDecision<AnnualTurnoverException>().Init(Trail.MyInputData.AnnualTurnover, Trail.MyInputData.AutoRejectionException_AnualTurnover);
			}
		}

		private void CheckBrokerClient()
		{
			if (Trail.MyInputData.IsBrokerClient)
			{
				StepNoReject<BrokerClientException>(true).Init(Trail.MyInputData.IsBrokerClient);
			}
			else
			{
				StepNoDecision<BrokerClientException>().Init(Trail.MyInputData.IsBrokerClient);
			}
		}

		private void CheckHighConsumerScore()
		{
			if (Trail.MyInputData.ConsumerScore > Trail.MyInputData.AutoRejectionException_CreditScore)
			{
				StepNoReject<ConsumerScoreException>(true).Init(Trail.MyInputData.ConsumerScore, Trail.MyInputData.AutoRejectionException_CreditScore);
			}
			else
			{
				StepNoDecision<ConsumerScoreException>().Init(Trail.MyInputData.ConsumerScore, Trail.MyInputData.AutoRejectionException_CreditScore);
			}
		}

		private void CheckHighBusinessScore()
		{
			if (Trail.MyInputData.BusinessScore > Trail.MyInputData.RejectionExceptionMaxCompanyScore)
			{
				StepNoReject<BusinessScoreException>(true).Init(Trail.MyInputData.BusinessScore, Trail.MyInputData.RejectionExceptionMaxCompanyScore);
			}
			else
			{
				StepNoDecision<BusinessScoreException>().Init(Trail.MyInputData.BusinessScore, Trail.MyInputData.RejectionExceptionMaxCompanyScore);
			}
		}

		private void CheckMpError()
		{
			var data = new MarketPlaceWithErrorException.DataModel()
			{
				HasMpError = Trail.MyInputData.HasMpError,
				MaxBusinessScore = Trail.MyInputData.BusinessScore,
				MaxBusinessScoreThreshhold = Trail.MyInputData.RejectionExceptionMaxCompanyScoreForMpError,
				MaxConsumerScore = Trail.MyInputData.ConsumerScore,
				MaxConsumerScoreThreshhold = Trail.MyInputData.RejectionExceptionMaxConsumerScoreForMpError
			};

			if (Trail.MyInputData.HasMpError &&
				(Trail.MyInputData.ConsumerScore > Trail.MyInputData.RejectionExceptionMaxConsumerScoreForMpError ||
				 Trail.MyInputData.BusinessScore > Trail.MyInputData.RejectionExceptionMaxCompanyScoreForMpError))
			{
				StepNoReject<MarketPlaceWithErrorException>(true).Init(data);
			}
			else
			{
				StepNoDecision<MarketPlaceWithErrorException>().Init(data);
			}
		}

		#endregion

		#region rejection checks
		/// <summary>
		/// rejection steps - if one of the steps determine reject - the client will be rejected (if none of the rejection exception rules where true)
		/// </summary>
		private void CheckRejections()
		{
			CheckLowConsumerScore();
			CheckLowBusinessScore();
			CheckConsumerDefaults();
			CheckCompanyDefaults();
			CheckSeniority();
			CheckCustomerStatus();
			CheckLowTurnover();
			CheckConsumerLates();
		}

		private void CheckLowConsumerScore()
		{
			if (Trail.MyInputData.ConsumerScore > 0 && Trail.MyInputData.ConsumerScore < Trail.MyInputData.LowCreditScore)
			{
				StepReject<ConsumerScore>(true).Init(Trail.MyInputData.ConsumerScore, 0, Trail.MyInputData.LowCreditScore, false);
			}
			else
			{
				StepNoDecision<ConsumerScore>().Init(Trail.MyInputData.ConsumerScore, 0, Trail.MyInputData.LowCreditScore, false);
			}
		}

		private void CheckLowBusinessScore()
		{
			if (Trail.MyInputData.BusinessScore > 0 && Trail.MyInputData.BusinessScore < Trail.MyInputData.RejectionCompanyScore)
			{
				StepReject<BusinessScore>(true).Init(Trail.MyInputData.BusinessScore, 0, Trail.MyInputData.RejectionCompanyScore, false);
			}
			else
			{
				StepNoDecision<BusinessScore>().Init(Trail.MyInputData.BusinessScore, 0, Trail.MyInputData.RejectionCompanyScore, false);
			}
		}

		private void CheckConsumerDefaults()
		{
			var data = new ConsumerDefaults.DataModel
			{
				MaxConsumerScore = Trail.MyInputData.ConsumerScore,
				MaxConsumerScoreThreshhold = Trail.MyInputData.Reject_Defaults_CreditScore,
				AmountOfDefaults = Trail.MyInputData.DefaultAmountInConsumerAccounts,
				AmountDefaultAccountsThreshhold = Trail.MyInputData.Reject_Defaults_Amount,
				NumOfDefaults = Trail.MyInputData.NumOfDefaultConsumerAccounts,
				NumDefaultAccountsThreshhold = Trail.MyInputData.Reject_Defaults_AccountsNum
			};

			if (Trail.MyInputData.NumOfDefaultConsumerAccounts >= Trail.MyInputData.Reject_Defaults_AccountsNum &&
				Trail.MyInputData.DefaultAmountInConsumerAccounts > Trail.MyInputData.Reject_Defaults_Amount &&
				Trail.MyInputData.ConsumerScore < Trail.MyInputData.Reject_Defaults_CreditScore)
			{
				StepReject<ConsumerDefaults>(true).Init(data);
			}
			else
			{
				StepNoDecision<ConsumerDefaults>().Init(data);
			}
		}

		private void CheckCompanyDefaults()
		{
			var data = new BusinessDefaults.DataModel
			{
				MaxBusinessScore = Trail.MyInputData.BusinessScore,
				MaxBusinessScoreThreshhold = Trail.MyInputData.Reject_Defaults_CompanyScore,
				AmountOfDefaults = Trail.MyInputData.DefaultAmountInBusinessAccounts,
				AmountDefaultAccountsThreshhold = Trail.MyInputData.Reject_Defaults_CompanyAmount,
				NumOfDefaults = Trail.MyInputData.NumOfDefaultBusinessAccounts,
				NumDefaultAccountsThreshhold = Trail.MyInputData.Reject_Defaults_CompanyAccountsNum
			};

			if (Trail.MyInputData.NumOfDefaultBusinessAccounts >= Trail.MyInputData.Reject_Defaults_CompanyAccountsNum &&
				Trail.MyInputData.DefaultAmountInBusinessAccounts > Trail.MyInputData.Reject_Defaults_CompanyAmount &&
				Trail.MyInputData.BusinessScore < Trail.MyInputData.Reject_Defaults_CompanyScore)
			{
				StepReject<BusinessDefaults>(true).Init(data);
			}
			else
			{
				StepNoDecision<BusinessDefaults>().Init(data);
			}
		}

		private void CheckSeniority()
		{
			if (Trail.MyInputData.BusinessSeniorityDays > 0 && Trail.MyInputData.BusinessSeniorityDays < Trail.MyInputData.Reject_Minimal_Seniority)
			{
				StepReject<Seniority>(true).Init(Trail.MyInputData.BusinessSeniorityDays, 0, Trail.MyInputData.Reject_Minimal_Seniority, false);
			}
			else
			{
				StepNoDecision<BusinessScore>().Init(Trail.MyInputData.BusinessSeniorityDays, 0, Trail.MyInputData.Reject_Minimal_Seniority, false);
			}
		}

		private void CheckCustomerStatus()
		{
			if (Trail.MyInputData.CustomerStatus == "Enabled" || Trail.MyInputData.CustomerStatus == "Fraud Suspect")
			{
				StepNoDecision<CustomerStatus>().Init(Trail.MyInputData.CustomerStatus);
			}
			else
			{
				StepReject<CustomerStatus>(true).Init(Trail.MyInputData.CustomerStatus);
			}
		}

		private void CheckLowTurnover()
		{
			var data = new Turnover.DataModel
			{
				AnnualTurnover = (int)Trail.MyInputData.AnnualTurnover,
				AnnualTurnoverThreshhold = Trail.MyInputData.TotalAnnualTurnover,
				QuarterTurnover = (int)Trail.MyInputData.QuarterTurnover,
				QuarterTurnoverThreshhold = Trail.MyInputData.TotalThreeMonthTurnover,
				HasCompanyFiles = Trail.MyInputData.HasCompanyFiles
			};

			if ((data.AnnualTurnover < data.AnnualTurnoverThreshhold || data.QuarterTurnover<data.QuarterTurnoverThreshhold) && !data.HasCompanyFiles)
			{
				StepReject<Turnover>(true).Init(data);
			}
			else
			{
				StepNoDecision<Turnover>().Init(data);
			}
		}

		private void CheckConsumerLates()
		{
			var data = new ConsumerLates.DataModel
			{
				LateDays = Trail.MyInputData.ConsumerLateDays,
				LateDaysThreshhold = Trail.MyInputData.RejectionLastValidLate,
				NumOfLates = Trail.MyInputData.NumOfLateConsumerAccounts,
				NumOfLatesThreshhold = Trail.MyInputData.Reject_NumOfLateAccounts

			};

			if (data.LateDays > data.LateDaysThreshhold && data.NumOfLates >= data.NumOfLatesThreshhold)
			{
				StepReject<ConsumerLates>(true).Init(data);
			}
			else
			{
				StepNoDecision<ConsumerLates>().Init(data);
			}
		}
		#endregion

		#endregion steps

		private T StepReject<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace
		{
			return Trail.Affirmative<T>(bLockDecisionAfterAddingAStep);
		} // StepReject

		private T StepNoReject<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace
		{
			return Trail.Negative<T>(bLockDecisionAfterAddingAStep);
		} // StepNoReject

		private T StepNoDecision<T>() where T : ATrace
		{
			return Trail.Dunno<T>();
		} // StepReject


		#region fields

		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		private int _customerId;
		private bool _isAutoRejected;
		private RejectionConfigs _configs;

		#endregion fields

		#endregion private
	} // class Agent
} // namespace
