namespace AutomationCalculator.MedalCalculation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Common;
	using Ezbob.Logger;

	public class OnlineLImitedMedalCalculator : MedalCalculator
	{
		public OnlineLImitedMedalCalculator(ASafeLog log):base(log) {}

		public override MedalInputModel GetInputParameters(int customerId) {
			var dbHelper = new DbHelper(Log);
			var dbData = dbHelper.GetMedalInputModel(customerId);
			var model = new MedalInputModel();
			if (dbData.HasMoreThanOneHmrc) {
				model.HasMoreThanOneHmrc = dbData.HasMoreThanOneHmrc;
				return model;
			}

			var yodlees = dbHelper.GetCustomerYodlees(customerId);
			var yodleeIncome = MarketPlacesHelper.GetYodleeAnnualized(yodlees, Log);
			var today = DateTime.Today;
			const int year = 365;

			model.HasHmrc = dbData.HasHmrc;
			model.BusinessScore = dbData.BusinessScore;
			model.BusinessSeniority = (decimal)(today - dbData.IncorporationDate).TotalDays/year;
			model.ConsumerScore = dbData.ConsumerScore;
			model.EzbobSeniority = ((today.Year - dbData.RegistrationDate.Year)*12) + today.Month - dbData.RegistrationDate.Month;
			model.FirstRepaymentDatePassed = dbData.FirstRepaymentDate.HasValue && dbData.FirstRepaymentDate.Value < today;
			model.IsLimited = dbData.TypeOfBusiness == "Limited" || dbData.TypeOfBusiness == "LLP";
			model.NumOfEarlyPayments = dbData.NumOfEarlyPayments;
			model.NumOfLatePayments = dbData.NumOfLatePayments;
			model.NumOfOnTimeLoans = dbData.NumOfOnTimeLoans;
			model.MaritalStatus = (MaritalStatus)Enum.Parse(typeof(MaritalStatus), dbData.MaritalStatus);
			model.AnnualTurnover = dbData.HasHmrc ? dbData.HmrcRevenues : yodleeIncome;
			model.AnnualTurnover = model.AnnualTurnover < 0 ? 0 : model.AnnualTurnover;

			var balance = dbData.CurrentBalanceSum < 0 ? 0 : (dbData.CurrentBalanceSum / dbData.FCFFactor);
			model.FreeCashFlow = model.AnnualTurnover == 0 ? 0 : (dbData.HmrcEbida - balance) / model.AnnualTurnover;
			model.TangibleEquity = model.AnnualTurnover == 0 ? 0 : dbData.TangibleEquity/model.AnnualTurnover;
			model.NetWorth = dbData.ZooplaValue == 0 ? 0 : (dbData.ZooplaValue - dbData.Mortages) / (decimal)dbData.ZooplaValue;

			return model;
		}

		public override MedalOutputModel CalculateMedal(MedalInputModel model)
		{
			Log.Debug(model.ToString());

			var dict = new Dictionary<Parameter, Weight>
				{
					
					{Parameter.BusinessScore,            GetBusinessScoreWeight(model.BusinessScore, model.FirstRepaymentDatePassed, model.HasHmrc)},
					{Parameter.TangibleEquity,           GetTangibleEquityWeight(model.TangibleEquity)},
					{Parameter.BusinessSeniority,        GetBusinessSeniorityWeight(model.BusinessSeniority, model.FirstRepaymentDatePassed, model.HasHmrc)},
					{Parameter.ConsumerScore,            GetConsumerScoreWeight(model.ConsumerScore, model.FirstRepaymentDatePassed, model.HasHmrc)},
					{Parameter.EzbobSeniority,           GetEzbobSeniorityWeight(model.EzbobSeniority, model.FirstRepaymentDatePassed)},
					{Parameter.MaritalStatus,            GetMaritalStatusWeight(model.MaritalStatus)},
					{Parameter.NumOfOnTimeLoans,         GetNumOfOnTimeLoansWeight(model.NumOfOnTimeLoans, model.FirstRepaymentDatePassed)},
					{Parameter.NumOfLatePayments,        GetNumOfLatePaymentsWeight(model.NumOfLatePayments, model.FirstRepaymentDatePassed)},
					{Parameter.NumOfEarlyPayments,       GetNumOfEarlyPaymentsWeight(model.NumOfEarlyPayments, model.FirstRepaymentDatePassed)},
					{Parameter.AnnualTurnover,           GetAnnualTurnoverWeight(model.AnnualTurnover, model.HasHmrc)},
					{Parameter.FreeCashFlow,             GetFreeCashFlowWeight(model.FreeCashFlow, model.HasHmrc)},
					{Parameter.NetWorth,                 GetNetWorthWeight(model.NetWorth)},
					{Parameter.NumOfStores,              GetNumOfStoresWeight(model.NumOfStores)},
					{Parameter.PositiveFeedbacks,        GetPositiveFeedbacksWeight(model.PositiveFeedbacks)}
				};

			CalcDelta(model, dict);

			MedalOutputModel scoreMedal = CalcScoreMedalOffer(dict);

			return scoreMedal;
		}

		private MedalOutputModel CalcScoreMedalOffer(Dictionary<Parameter, Weight> dict)
		{
			decimal minScoreSum = 0M;
			decimal maxScoreSum = 0M;
			decimal scoreSum = 0M;
			foreach (var weight in dict.Values)
			{
				weight.Score = weight.Grade*weight.FinalWeight;
				minScoreSum += weight.MinimumScore;
				maxScoreSum += weight.MaximumScore;
				scoreSum += weight.Score;
			}

			decimal score = (scoreSum - minScoreSum)/(maxScoreSum - minScoreSum);
			Medal medal = GetMedal(MedalRangesConstats.MedalRanges, score);
			var medalOutput = new MedalOutputModel
				{
					Medal = medal,
					Score = score,
				};

			PrintDict(medalOutput, dict);
			return medalOutput;
		}

		private void CalcDelta(MedalInputModel model, Dictionary<Parameter, Weight> dict)
		{

			if (model.BusinessScore <= OnlineLimitedMedalConstants.LowBusinessScore || model.ConsumerScore <= OnlineLimitedMedalConstants.LowConsumerScore) {
				//Sum of all weights
				var sow = dict.Sum(x => x.Value.FinalWeight);
				//Sum of weights of of TangibleEquity, NetWorth, MaritalStatus, NumberOfStores, PositiveFeedbacks
				var sonf = dict[Parameter.TangibleEquity].FinalWeight + 
					dict[Parameter.NetWorth].FinalWeight + 
					dict[Parameter.MaritalStatus].FinalWeight + 
					dict[Parameter.NumOfStores].FinalWeight + 
					dict[Parameter.PositiveFeedbacks].FinalWeight;

				var sonfDesired = sonf - sow + 1;
				var ratio = sonfDesired/sonf;
				dict[Parameter.TangibleEquity].FinalWeight *= ratio;
				dict[Parameter.NetWorth].FinalWeight *= ratio;
				dict[Parameter.MaritalStatus].FinalWeight *= ratio;
				dict[Parameter.NumOfStores].FinalWeight *= ratio;
				dict[Parameter.PositiveFeedbacks].FinalWeight *= ratio;
			}

			foreach (var weight in dict.Values)
			{
				weight.MinimumScore = weight.FinalWeight * weight.MinimumGrade;
				weight.MaximumScore = weight.FinalWeight * weight.MaximumGrade;
				weight.Score = weight.FinalWeight * weight.Score;
			}
		}

		private Weight GetNumOfEarlyPaymentsWeight(int ezbobNumOfEarlyReayments, bool firstRepaymentDatePassed)
		{
			return GetBaseWeight(ezbobNumOfEarlyReayments, OnlineLimitedMedalConstants.NumOfEarlyRepaymentsBaseWeight, MedalRangesConstats.NumOfEarlyRepaymentsRanges,
				firstRepaymentDatePassed, OnlineLimitedMedalConstants.NumOfEarlyRepaymentsFirstRepaymentWeight);
		}

		private Weight GetNumOfLatePaymentsWeight(int ezbobNumOfLateRepayments, bool firstRepaymentDatePassed)
		{
			return GetBaseWeight(ezbobNumOfLateRepayments, OnlineLimitedMedalConstants.NumOfLateRepaymentsBaseWeight, MedalRangesConstats.NumOfLateRepaymentsRanges,
				firstRepaymentDatePassed, OnlineLimitedMedalConstants.NumOfLateRepaymentsFirstRepaymentWeight);
		}

		private Weight GetNumOfOnTimeLoansWeight(int ezbobNumOfLoans, bool firstRepaymentDatePassed)
		{
			return GetBaseWeight(ezbobNumOfLoans, OnlineLimitedMedalConstants.NumOfOnTimeLoansBaseWeight, MedalRangesConstats.NumOfOnTimeLoansRanges,
				firstRepaymentDatePassed, OnlineLimitedMedalConstants.NumOfOnTimeLoansFirstRepaymentWeight);
		}

		private Weight GetEzbobSeniorityWeight(decimal ezbobSeniority, bool firstRepaymentDatePassed)
		{
			return GetBaseWeight(ezbobSeniority, OnlineLimitedMedalConstants.EzbobSeniorityBaseWeight, MedalRangesConstats.EzbobSeniorityRanges, 
				firstRepaymentDatePassed, OnlineLimitedMedalConstants.EzbobSeniorityFirstRepaymentWeight);
		}

		private Weight GetPositiveFeedbacksWeight(int positiveFeedbacks)
		{
			return GetBaseWeight(positiveFeedbacks, OnlineLimitedMedalConstants.PositiveFeedbacksBaseWeight, MedalRangesConstats.PositiveFeedbacksRanges);
		}

		private Weight GetNumOfStoresWeight(int numOfStores)
		{
			return GetBaseWeight(numOfStores, OnlineLimitedMedalConstants.NumOfStoresBaseWeight, MedalRangesConstats.NumOfStoresRanges);
		}
		
		private Weight GetAnnualTurnoverWeight(decimal annualTurnover, bool hasHmrc) {
			var annualTurnoverWeight = GetBaseWeight(annualTurnover, OnlineLimitedMedalConstants.AnnualTurnoverBaseWeight, MedalRangesConstats.AnnualTurnoverRanges);
				
			if (!hasHmrc) {
				annualTurnoverWeight.FinalWeight += OnlineLimitedMedalConstants.AnnualTurnoverNoHmrcWeightChange;
			}

			return annualTurnoverWeight;
		}
		
		private Weight GetMaritalStatusWeight(MaritalStatus maritalStatus)
		{
			var maritalStatusWeight = new Weight
			{
				FinalWeight = OnlineLimitedMedalConstants.MaritalStatusBaseWeight,
				MinimumGrade = MedalRangesConstats.MaritalStatusGrade_Single,
				MaximumGrade = MedalRangesConstats.MaritalStatusGrade_Married
			};

			switch (maritalStatus)
			{
				case MaritalStatus.Married:
					maritalStatusWeight.Grade = MedalRangesConstats.MaritalStatusGrade_Married;
					break;
				case MaritalStatus.Divorced:
					maritalStatusWeight.Grade = MedalRangesConstats.MaritalStatusGrade_Divorced;
					break;
				case MaritalStatus.Single:
				case MaritalStatus.Other:
					maritalStatusWeight.Grade = MedalRangesConstats.MaritalStatusGrade_Single;
					break;
				case MaritalStatus.Widowed:
					maritalStatusWeight.Grade = MedalRangesConstats.MaritalStatusGrade_Widowed;
					break;
				case MaritalStatus.LivingTogether:
					maritalStatusWeight.Grade = MedalRangesConstats.MaritalStatusGrade_LivingTogether;
					break;
				case MaritalStatus.Separated:
					maritalStatusWeight.Grade = MedalRangesConstats.MaritalStatusGrade_Separated;
					break;
				default:
					maritalStatusWeight.Grade = MedalRangesConstats.MaritalStatusGrade_Other;
					break;
			}

			return maritalStatusWeight;
		}

		private Weight GetNetWorthWeight(decimal netWorth)
		{
			return GetBaseWeight(netWorth, OnlineLimitedMedalConstants.NetWorthBaseWeight, MedalRangesConstats.NetWorthRanges);
		}

		private Weight GetFreeCashFlowWeight(decimal freeCashFlow, bool hasHmrc)
		{
			var freeCashFlowWeight = GetBaseWeight(freeCashFlow, OnlineLimitedMedalConstants.FreeCashFlowBaseWeight, MedalRangesConstats.FreeCashFlowRanges);
			
			if (!hasHmrc)
			{
				freeCashFlowWeight.FinalWeight = OnlineLimitedMedalConstants.FreeCashFlowNoHmrcWeight;
			}

			return freeCashFlowWeight;
		}

		private Weight GetBusinessSeniorityWeight(decimal businessSeniority, bool firstRepaymentDatePassed, bool hasHmrc)
		{
			var businessSeniorityWeight = GetBaseWeight(businessSeniority, OnlineLimitedMedalConstants.BusinessSeniorityBaseWeight, MedalRangesConstats.BusinessSeniorityRanges);
			
			if (!hasHmrc) {
				businessSeniorityWeight.FinalWeight += OnlineLimitedMedalConstants.BusinessSeniorityNoHmrcWeightChange;
			}

			if (firstRepaymentDatePassed) {
				businessSeniorityWeight.FinalWeight += OnlineLimitedMedalConstants.BusinessSeniorityFirstRepaymentWeightChange;
			}

			return businessSeniorityWeight;
		}

		private Weight GetTangibleEquityWeight(decimal tangibleEquity)
		{
			return GetBaseWeight(tangibleEquity, OnlineLimitedMedalConstants.TangibleEquityBaseWeight, MedalRangesConstats.TangibleEquityRanges);
		}

		private Weight GetBusinessScoreWeight(int businessScore, bool firstRepaymentDatePassed, bool hasHmrc)
		{
			var businessScoreWeight = GetBaseWeight(businessScore, OnlineLimitedMedalConstants.BusinessScoreBaseWeight, MedalRangesConstats.BusinessScoreRanges);
			
			if (businessScore <= OnlineLimitedMedalConstants.LowBusinessScore)
			{
				businessScoreWeight.FinalWeight = OnlineLimitedMedalConstants.BusinessScoreLowScoreWeight;
			}

			if (!hasHmrc)
			{
				businessScoreWeight.FinalWeight += OnlineLimitedMedalConstants.BusinessScoreNoHmrcWeightChange;
			}

			if (firstRepaymentDatePassed)
			{
				businessScoreWeight.FinalWeight += OnlineLimitedMedalConstants.BusinessScoreFirstRepaymentWeightChange;
			}

			return businessScoreWeight;
		}

		private Weight GetConsumerScoreWeight(int consumerScore, bool firstRepaymentDatePassed, bool hasHmrc)
		{
			var consumerScoreWeight = GetBaseWeight(consumerScore, OnlineLimitedMedalConstants.ConsumerScoreBaseWeight, MedalRangesConstats.ConsumerScoreRanges);
			
			if (consumerScore <= OnlineLimitedMedalConstants.LowConsumerScore) {
				consumerScoreWeight.FinalWeight = OnlineLimitedMedalConstants.ConsumerScoreLowScoreWeight;
			}

			if (!hasHmrc) {
				consumerScoreWeight.FinalWeight += OnlineLimitedMedalConstants.ConsumerScoreNoHmrcWeightChange;
			}

			if (firstRepaymentDatePassed) {
				consumerScoreWeight.FinalWeight += OnlineLimitedMedalConstants.ConsumerScoreFirstRepaymentWeightChange;
			}

			return consumerScoreWeight;
		}
	}
}
