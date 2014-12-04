﻿namespace AutomationCalculator.AutoDecision.AutoRejection {
	using System;
	using Newtonsoft.Json;
	using ProcessHistory;

	public class AutoRejectionInputDataModelDb {
		//from sp
		public string CustomerStatus { get; set; }
		public int ExperianScore { get; set; }
		public int CompanyScore { get; set; }
		public bool WasApproved { get; set; }
		public bool IsBrokerClient { get; set; }
		public bool HasErrorMp { get; set; }
		public bool HasCompanyFiles { get; set; }
		public DateTime? IncorporationDate { get; set; }
		public DateTime? ConsumerDataTime { get; set; }
	} // class AutoRejectionInputDataModelDb

	public class RejectionConfigs {
		public int AutoRejectConsumerCheckAge { get; set; }
		public int AutoRejectionException_AnualTurnover { get; set; }
		public int AutoRejectionException_CreditScore { get; set; }
		public int RejectionExceptionMaxCompanyScore { get; set; }
		public int RejectionExceptionMaxConsumerScoreForMpError { get; set; }
		public int RejectionExceptionMaxCompanyScoreForMpError { get; set; }
		public int LowCreditScore { get; set; }
		public int RejectionCompanyScore { get; set; }
		public int Reject_Defaults_CreditScore { get; set; }
		public int Reject_Defaults_AccountsNum { get; set; }
		public int Reject_Defaults_Amount { get; set; }
		public int Reject_Defaults_MonthsNum { get; set; }

		public int Reject_Defaults_CompanyScore { get; set; }
		public int Reject_Defaults_CompanyAccountsNum { get; set; }
		public int Reject_Defaults_CompanyMonthsNum { get; set; }
		public int Reject_Defaults_CompanyAmount { get; set; }
		public int Reject_Minimal_Seniority { get; set; }
		public int TotalAnnualTurnover { get; set; }
		public int TotalThreeMonthTurnover { get; set; }
		public int Reject_LateLastMonthsNum { get; set; }
		public int Reject_NumOfLateAccounts { get; set; }
		public int RejectionLastValidLate { get; set; }
	} // class RejectionConfigs

	public class RejectionInputData : RejectionConfigs, ITrailInputData {
		public DateTime DataAsOf { get; private set; }

		public bool WasApproved { get; set; }
		public bool IsBrokerClient { get; set; }
		public decimal AnnualTurnover { get; set; }
		public decimal QuarterTurnover { get; set; }
		public int ConsumerScore { get; set; }
		public int BusinessScore { get; set; }
		public bool HasMpError { get; set; }
		public bool HasCompanyFiles { get; set; }
		public int NumOfDefaultConsumerAccounts { get; set; }
		public int DefaultAmountInConsumerAccounts { get; set; }
		public int NumOfDefaultBusinessAccounts { get; set; }
		public int DefaultAmountInBusinessAccounts { get; set; }
		public int BusinessSeniorityDays { get; set; }
		public string CustomerStatus { get; set; }
		public int NumOfLateConsumerAccounts { get; set; }
		public int ConsumerLateDays { get; set; }
		public DateTime? ConsumerDataTime { get; set; }

		#region method InitCfg

		public void InitCfg(DateTime dataAsOf, RejectionConfigs configs) {
			DataAsOf = dataAsOf;

			AutoRejectionException_AnualTurnover = configs.AutoRejectionException_AnualTurnover;
			AutoRejectionException_CreditScore = configs.AutoRejectionException_CreditScore;
			RejectionExceptionMaxCompanyScore = configs.RejectionExceptionMaxCompanyScore;
			RejectionExceptionMaxConsumerScoreForMpError = configs.RejectionExceptionMaxConsumerScoreForMpError;
			RejectionExceptionMaxCompanyScoreForMpError = configs.RejectionExceptionMaxCompanyScoreForMpError;
			LowCreditScore = configs.LowCreditScore;
			RejectionCompanyScore = configs.RejectionCompanyScore;
			Reject_Defaults_CreditScore = configs.Reject_Defaults_CreditScore;
			Reject_Defaults_AccountsNum = configs.Reject_Defaults_AccountsNum;
			Reject_Defaults_Amount = configs.Reject_Defaults_Amount;
			Reject_Defaults_MonthsNum = configs.Reject_Defaults_MonthsNum;
			Reject_Defaults_CompanyScore = configs.Reject_Defaults_CompanyScore;
			Reject_Defaults_CompanyAccountsNum = configs.Reject_Defaults_CompanyAccountsNum;
			Reject_Defaults_CompanyMonthsNum = configs.Reject_Defaults_CompanyMonthsNum;
			Reject_Defaults_CompanyAmount = configs.Reject_Defaults_CompanyAmount;
			Reject_Minimal_Seniority = configs.Reject_Minimal_Seniority;
			TotalAnnualTurnover = configs.TotalAnnualTurnover;
			TotalThreeMonthTurnover = configs.TotalThreeMonthTurnover;
			Reject_LateLastMonthsNum = configs.Reject_LateLastMonthsNum;
			Reject_NumOfLateAccounts = configs.Reject_NumOfLateAccounts;
			RejectionLastValidLate = configs.RejectionLastValidLate;
		} // InitCfg

		#endregion method InitCfg

		#region method InitData

		public void InitData(RejectionInputData data) {
			WasApproved = data.WasApproved;
			IsBrokerClient = data.IsBrokerClient;
			AnnualTurnover = data.AnnualTurnover;
			QuarterTurnover = data.QuarterTurnover;
			ConsumerScore = data.ConsumerScore;
			BusinessScore = data.BusinessScore;
			HasMpError = data.HasMpError;
			HasCompanyFiles = data.HasCompanyFiles;
			CustomerStatus = data.CustomerStatus;

			NumOfDefaultConsumerAccounts = data.NumOfDefaultConsumerAccounts;
			DefaultAmountInConsumerAccounts = data.DefaultAmountInConsumerAccounts;
			NumOfDefaultBusinessAccounts = data.NumOfDefaultBusinessAccounts;
			DefaultAmountInBusinessAccounts = data.DefaultAmountInBusinessAccounts;
			NumOfLateConsumerAccounts = data.NumOfLateConsumerAccounts;
			ConsumerLateDays = data.ConsumerLateDays;

			BusinessSeniorityDays = data.BusinessSeniorityDays;
			ConsumerDataTime = data.ConsumerDataTime;
		} // InitData

		#endregion method InitData

		#region method Init

		public void Init(DateTime dataAsOf, RejectionInputData data, RejectionConfigs configs) {
			InitCfg(dataAsOf, configs);
			InitData(data);
		} // Init

		#endregion method Init

		public string Serialize() {
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		} // Serialize

		[JsonIgnore]
		public DateTime MonthsNumAgo {
			get { return DataAsOf.AddMonths(-1 * Reject_Defaults_MonthsNum); }
		} // MonthsNumAgo

		[JsonIgnore]
		public DateTime CompanyMonthsNumAgo {
			get { return DataAsOf.AddMonths(-1 * Reject_Defaults_CompanyMonthsNum); }
		} // CompanyMonthsNumAgo

		[JsonIgnore]
		public bool ConsumerDataIsTooOld {
			get {
				if (ConsumerDataTime == null)
					return true;

				return ConsumerDataTime.Value < DataAsOf.AddMonths(-AutoRejectConsumerCheckAge);
			} // get
		} // ConsumerDataIsTooOld
	} // RejectionInputData
} // namespace
