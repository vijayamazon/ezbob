namespace AutomationCalculator.AutoDecision.AutoRejection
{
	using System;
	using AutoApproval;
	using ProcessHistory.Trails;
	using Common;
	using ProcessHistory;
	using ProcessHistory.Common;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class RejectionAgent
	{
		private readonly DbHelper _dbHelper;
		private readonly MarketPlacesHelper _mpHelper;

		#region public

		#region constructor

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
			};
		
			model.Init(now, data, _configs);
			return model;
		}

		#region method MakeDecision

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

			m_oLog.Debug(
				"Checking if auto rejection should take place for customer {0} complete; {1}\n{2}",
				_customerId,
				Trail,
				_isAutoRejected ? "Auto rejected" : "Not auto rejected"
			);
		}

		private void CheckRejectionExceptions()
		{

		}

		private void CheckRejections() {
			
		}

		

// MakeDecision

		#endregion method MakeDecision

		public RejectionTrail Trail { get; private set; }

		#endregion public

		#region private

		#region steps


		#endregion steps

		#region method ProcessRow

		private void ProcessRow(SafeReader sr)
		{

		} // ProcessRow

		#endregion method ProcessRow
		
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

		private readonly DateTime Now;
		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		private int _customerId;
		private bool _isAutoRejected;
		private RejectionConfigs _configs;

		#endregion fields

		#endregion private
	} // class Agent
} // namespace
