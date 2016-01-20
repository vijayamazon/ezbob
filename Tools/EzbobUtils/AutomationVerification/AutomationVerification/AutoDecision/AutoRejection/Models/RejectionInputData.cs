namespace AutomationCalculator.AutoDecision.AutoRejection.Models {
	using System;
	using AutomationCalculator.ProcessHistory;
	using Newtonsoft.Json;

	public class RejectionInputData : RejectionConfigs, ITrailInputData {
		public virtual DateTime DataAsOf { get; private set; }

		public virtual bool WasApproved { get; set; }
		public virtual bool IsBrokerClient { get; set; }
		public virtual decimal AnnualTurnover { get; set; }
		public virtual decimal QuarterTurnover { get; set; }
		public virtual int ConsumerScore { get; set; }
		public virtual int BusinessScore { get; set; }
		public virtual bool HasMpError { get; set; }
		public virtual bool HasCompanyFiles { get; set; }
		public virtual int NumOfDefaultConsumerAccounts { get; set; }
		public virtual int DefaultAmountInConsumerAccounts { get; set; }
		public virtual int NumOfDefaultBusinessAccounts { get; set; }
		public virtual int DefaultAmountInBusinessAccounts { get; set; }
		public virtual int BusinessSeniorityDays { get; set; }
		public virtual string CustomerStatus { get; set; }
		public virtual int NumOfLateConsumerAccounts { get; set; }
		public virtual int ConsumerLateDays { get; set; }
		public virtual DateTime? ConsumerDataTime { get; set; }

		public virtual void InitCfg(DateTime dataAsOf, RejectionConfigs configs) {
			if (configs == null)
				throw new ArgumentNullException("configs", "No configuration to copy from specified.");

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
			AutoRejectConsumerCheckAge = configs.AutoRejectConsumerCheckAge;

			EnabledTraces.Clear();

			foreach (var s in configs.EnabledTraces)
				EnabledTraces.Add(s);
		} // InitCfg

		public virtual void InitData(RejectionInputData data) {
			if (data == null)
				throw new ArgumentNullException("data", "No data to copy from specified.");

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

		public virtual void Init(DateTime dataAsOf, RejectionInputData data, RejectionConfigs configs) {
			InitCfg(dataAsOf, configs);
			InitData(data);
		} // Init

		public virtual string Serialize() {
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		} // Serialize

		[JsonIgnore]
		public virtual DateTime MonthsNumAgo {
			get { return DataAsOf.AddMonths(-1 * Reject_Defaults_MonthsNum); }
		} // MonthsNumAgo

		[JsonIgnore]
		public virtual DateTime CompanyMonthsNumAgo {
			get { return DataAsOf.AddMonths(-1 * Reject_Defaults_CompanyMonthsNum); }
		} // CompanyMonthsNumAgo

		[JsonIgnore]
		public virtual bool ConsumerDataIsTooOld {
			get {
				if (ConsumerDataTime == null)
					return true;

				return ConsumerDataTime.Value < DataAsOf.AddMonths(-AutoRejectConsumerCheckAge);
			} // get
		} // ConsumerDataIsTooOld
	} // RejectionInputData
} // namespace
