namespace AutomationCalculator.MedalCalculation
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using Common;
	using Ezbob.Logger;

	public interface IMedalCalulator
	{
		MedalInputModel GetInputParameters(int customerId, DateTime? calculationDate = null);
		MedalOutputModel CalculateMedal(MedalInputModel model);
		void PrintDict(MedalOutputModel medalOutput);
	}

	public interface IMedalWeightConstatns
	{
		#region Base Weight
		decimal BusinessScoreBaseWeight { get; }
		decimal FreeCashFlowBaseWeight { get; }
		decimal AnnualTurnoverBaseWeight { get; }
		decimal TangibleEquityBaseWeight { get; }
		decimal BusinessSeniorityBaseWeight { get; }
		decimal ConsumerScoreBaseWeight { get; }
		decimal NetWorthBaseWeight { get; }
		decimal MaritalStatusBaseWeight { get; }
		decimal NumOfStoresBaseWeight { get; }
		decimal PositiveFeedbacksBaseWeight { get; }
		decimal EzbobSeniorityBaseWeight { get; }
		decimal NumOfOnTimeLoansBaseWeight { get; }
		decimal NumOfLateRepaymentsBaseWeight { get; }
		decimal NumOfEarlyRepaymentsBaseWeight { get; }
		#endregion

		#region No HMRC Weight
		decimal FreeCashFlowNoHmrcWeight { get; }
		decimal AnnualTurnoverNoHmrcWeightChange { get; }
		decimal BusinessScoreNoHmrcWeightChange { get; }
		decimal ConsumerScoreNoHmrcWeightChange { get; }
		decimal BusinessSeniorityNoHmrcWeightChange { get; }
		#endregion

		#region Low Score Weight

		int LowBusinessScore { get; }
		int LowConsumerScore { get; }

		decimal BusinessScoreLowScoreWeight { get; }
		decimal ConsumerScoreLowScoreWeight { get; }

		#endregion

		#region First Repayment Passed Weight
		decimal EzbobSeniorityFirstRepaymentWeight { get; }
		decimal NumOfOnTimeLoansFirstRepaymentWeight { get; }
		decimal NumOfLateRepaymentsFirstRepaymentWeight { get; }
		decimal NumOfEarlyRepaymentsFirstRepaymentWeight { get; }

		decimal ConsumerScoreFirstRepaymentWeightChange { get; }
		decimal BusinessScoreFirstRepaymentWeightChange { get; }
		decimal BusinessSeniorityFirstRepaymentWeightChange { get; }
		decimal NetWorthFirstRepaymentWeightChange { get; }
		#endregion
	}

	public abstract class MedalCalculator : IMedalCalulator, IMedalWeightConstatns
	{
		protected readonly ASafeLog Log;

		protected MedalCalculator(ASafeLog log)
		{
			Log = log;
		}

		public virtual MedalInputModel GetInputParameters(int customerId, DateTime? calculationDate = null)
		{
			var dbHelper = new DbHelper(Log);
			var dbData = dbHelper.GetMedalInputModel(customerId);
			var model = new MedalInputModel {MedalInputModelDb = dbData};

			var today = calculationDate.HasValue ? calculationDate.Value : DateTime.Today;
			model.CalculationDate = today;

			
			
			model.HasHmrc = dbData.HasHmrc;
			model.UseHmrc = dbData.HasHmrc;
			model.BusinessScore = dbData.BusinessScore;

			const int defaultBusinessSeniority = 1;
			model.BusinessSeniority = dbData.IncorporationDate.HasValue ? today.Year - dbData.IncorporationDate.Value.Year : defaultBusinessSeniority;
			if (dbData.IncorporationDate.HasValue && dbData.IncorporationDate.Value > today.AddYears(-model.BusinessSeniority)) {
				model.BusinessSeniority--;
			}

			model.ConsumerScore = dbData.ConsumerScore;
			model.EzbobSeniority = ((today.Year - dbData.RegistrationDate.Year) * 12) + today.Month - dbData.RegistrationDate.Month;
			model.FirstRepaymentDatePassed = dbData.FirstRepaymentDate.HasValue && dbData.FirstRepaymentDate.Value < today;
			model.NumOfEarlyPayments = dbData.NumOfEarlyPayments;
			model.NumOfLatePayments = dbData.NumOfLatePayments;
			model.NumOfOnTimeLoans = dbData.NumOfOnTimeLoans;
			model.MaritalStatus = (MaritalStatus)Enum.Parse(typeof(MaritalStatus), dbData.MaritalStatus);
			model.NetWorth = dbData.ZooplaValue == 0 ? 0 : (dbData.ZooplaValue - dbData.Mortages) / (decimal)dbData.ZooplaValue;

			// This Logic is good only for OfflineLimited,SoleTrader,NonLimited Medals
			var yodlees = dbHelper.GetCustomerYodlees(customerId);
			var mpHelper = new MarketPlacesHelper(Log);
			var yodleeIncome = mpHelper.GetYodleeAnnualized(yodlees, Log);
			model.AnnualTurnover = dbData.HasHmrc ? dbData.HmrcRevenues : yodleeIncome;
			model.AnnualTurnover = model.AnnualTurnover < 0 ? 0 : model.AnnualTurnover;

			model.FreeCashFlowValue = dbData.HmrcFreeCashFlow - (dbData.CurrentBalanceSum / dbData.FCFFactor);
			model.FreeCashFlow = model.AnnualTurnover == 0 || !dbData.HasHmrc ? 0 : model.FreeCashFlowValue / model.AnnualTurnover;
			model.TangibleEquity = model.AnnualTurnover == 0 ? 0 : model.MedalInputModelDb.TangibleEquity / model.AnnualTurnover;
			model.ValueAdded = dbData.HmrcValueAdded;
			model.CustomerId = customerId;
			return model;
		}

		public abstract MedalOutputModel CalculateMedal(MedalInputModel model);

		protected abstract void CalcDelta(MedalInputModel model, Dictionary<Parameter, Weight> dict);

		/// <summary>
		/// If HMRC turnover > 0.7*OnlineTurnover use HMRC turnover, otherwise, if yodlee turnover > 0.7*OnlineTurnover use yodlee turnover, otherwise use online turnover 
		///If it is negative – use 0 instead
		///The 0.7 should be configurable via UW – config name: OnlineMedalTurnoverCutoff
		/// </summary>
		protected MedalInputModel GetOnlineInputParameters(int customerId, MedalInputModel intputModel)
		{
			var usingHmrc = false;
			var dbHelper = new DbHelper(Log);
			var mpHelper = new MarketPlacesHelper(Log);
			var yodlees = dbHelper.GetCustomerYodlees(customerId);
			var yodleeIncome = mpHelper.GetYodleeAnnualized(yodlees, Log);
			var onlineTurnover = mpHelper.GetOnlineTurnoverAnnualized(customerId);
			if (intputModel.MedalInputModelDb.HmrcRevenues > onlineTurnover * intputModel.MedalInputModelDb.OnlineMedalTurnoverCutoff)
			{
				usingHmrc = true;
				intputModel.AnnualTurnover = intputModel.MedalInputModelDb.HmrcRevenues;
			}
			else if (yodleeIncome > onlineTurnover*intputModel.MedalInputModelDb.OnlineMedalTurnoverCutoff) {
				intputModel.AnnualTurnover = yodleeIncome;
			}
			else {
				intputModel.AnnualTurnover = onlineTurnover;
			}

			if (onlineTurnover < 0)
			{
				intputModel.AnnualTurnover = 0;
			}

			intputModel.FreeCashFlow = intputModel.AnnualTurnover == 0 || !intputModel.HasHmrc ? 0 : intputModel.FreeCashFlowValue / intputModel.AnnualTurnover;
			
			intputModel.TangibleEquity = intputModel.AnnualTurnover == 0 ? 0 : intputModel.MedalInputModelDb.TangibleEquity / intputModel.AnnualTurnover;
			intputModel.NumOfStores = intputModel.MedalInputModelDb.NumOfStores;

			intputModel.PositiveFeedbacks = mpHelper.GetPositiveFeedbacks(customerId);
			intputModel.UseHmrc = usingHmrc;

			return intputModel;
		}

		protected MedalOutputModel CalcScoreMedalOffer(Dictionary<Parameter, Weight> dict, MedalInputModel inputModel, MedalType medalType = MedalType.NoMedal)
		{
			decimal minScoreSum = 0;
			decimal maxScoreSum = 0;
			decimal scoreSum = 0;
			foreach (var weight in dict.Values)
			{
				weight.MinimumScore = weight.FinalWeight * weight.MinimumGrade;
				weight.MaximumScore = weight.FinalWeight * weight.MaximumGrade;
				weight.Score = weight.Grade * weight.FinalWeight;
				minScoreSum += weight.MinimumScore;
				maxScoreSum += weight.MaximumScore;
				scoreSum += weight.Score;
			}

			decimal score = (scoreSum - minScoreSum) / (maxScoreSum - minScoreSum);
			Medal medal = GetMedal(MedalRangesConstats.MedalRanges, score);
			var medalOutput = new MedalOutputModel
				{
					Medal = medal,
					MedalType = medalType,
					NormalizedScore = score,
					Score = scoreSum,
					Dict = dict,
					AnnualTurnover = inputModel.AnnualTurnover,
					FreeCashflow = inputModel.FreeCashFlowValue,
					FirstRepaymentDatePassed = inputModel.FirstRepaymentDatePassed,
					ValueAdded = inputModel.ValueAdded,
					NumOfHmrcMps = inputModel.HasHmrc ? 1 : 0,
					CustomerId = inputModel.CustomerId,
					CalculationDate = inputModel.CalculationDate,
					UseHmrc = inputModel.UseHmrc,
				};

			//PrintDict(medalOutput, dict);
			return medalOutput;
		}

		protected Weight GetBaseWeight(decimal value, decimal baseWeight, List<RangeGrage> ranges, bool firstRepaymentDatePassed = false, decimal firstRepaymentWeight = 0)
		{
			var weight = new Weight
			{
				Value = value.ToString(CultureInfo.InvariantCulture),
				FinalWeight = baseWeight,
				MinimumGrade = ranges.MinGrade(),
				MaximumGrade = ranges.MaxGrade(),
				Grade = GetGrade(ranges, value),
			};

			if (firstRepaymentDatePassed)
			{
				weight.FinalWeight = firstRepaymentWeight;
			}

			return weight;
		}

		protected Medal GetMedal(IEnumerable<RangeMedal> rangeMedals, decimal value)
		{
			var range = rangeMedals.GetRange(value);
			if (range != null)
			{
				return range.Medal;
			}
			return Medal.NoMedal;
		}

		protected int GetGrade(IEnumerable<RangeGrage> rangeGrages, decimal value)
		{
			var range = rangeGrages.GetRange(value);
			if (range != null)
			{
				return range.Grade;
			}
			return 0;
		}

		public void PrintDict(MedalOutputModel medalOutput) {
			Dictionary<Parameter, Weight> dict = medalOutput.Dict;
			var sb = new StringBuilder();
			sb.AppendFormat("Medal Type {2} Medal: {0} NormalizedScore: {1}% Score: {3}\n", medalOutput.Medal, ToPercent(medalOutput.NormalizedScore), medalOutput.MedalType, medalOutput.Score);
			decimal s5 = 0M, s6 = 0M, s7 = 0M, s8 = 0M, s9 = 0M, s10 = 0M, s11 = 0M;
			sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}| {8} \n", "Parameter".PadRight(25), "Weight".PadRight(10), "MinScore".PadRight(10), "MaxScore".PadRight(10), "MinGrade".PadRight(10), "MaxGrade".PadRight(10), "Grade".PadRight(10), "Score".PadRight(10), "Value");
			foreach (var weight in dict)
			{

				sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}| {8}\n",
					weight.Key.ToString().PadRight(25),
					ToPercent(weight.Value.FinalWeight).PadRight(10),
					ToPercent(weight.Value.MinimumScore / 100).PadRight(10),
					ToPercent(weight.Value.MaximumScore / 100).PadRight(10),
					weight.Value.MinimumGrade.ToString(CultureInfo.InvariantCulture).PadRight(10),
					weight.Value.MaximumGrade.ToString(CultureInfo.InvariantCulture).PadRight(10),
					weight.Value.Grade.ToString(CultureInfo.InvariantCulture).PadRight(10),
					ToShort(weight.Value.Score*100).PadRight(10), weight.Value.Value);
				s5 += weight.Value.FinalWeight;
				s6 += weight.Value.MinimumScore;
				s7 += weight.Value.MaximumScore;
				s8 += weight.Value.MinimumGrade;
				s9 += weight.Value.MaximumGrade;
				s11 += weight.Value.Grade;
				s10 += weight.Value.Score;
			}
			sb.AppendLine("----------------------------------------------------------------------------------------------------------------------------------------");
			sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}|\n",
				"Sum".PadRight(25),
				ToPercent(s5).PadRight(10),
				ToPercent(s6 / 100).PadRight(10),
				ToPercent(s7 / 100).PadRight(10),
				s8.ToString(CultureInfo.InvariantCulture).PadRight(10),
				s9.ToString(CultureInfo.InvariantCulture).PadRight(10),
				s11.ToString(CultureInfo.InvariantCulture).PadRight(10),
				ToShort(s10*100).PadRight(10));

			Log.Debug(sb.ToString());
		}

		protected string ToPercent(decimal val)
		{
			return String.Format("{0:F2}", val * 100).PadRight(6);
		}

		protected string ToShort(decimal val)
		{
			return String.Format("{0:F2}", val).PadRight(6);
		}

		#region GetWeight Methods
		protected virtual Weight GetNumOfEarlyPaymentsWeight(int ezbobNumOfEarlyReayments, bool firstRepaymentDatePassed)
		{
			return GetBaseWeight(ezbobNumOfEarlyReayments, NumOfEarlyRepaymentsBaseWeight, MedalRangesConstats.NumOfEarlyRepaymentsRanges,
				firstRepaymentDatePassed, NumOfEarlyRepaymentsFirstRepaymentWeight);
		}

		protected virtual Weight GetNumOfLatePaymentsWeight(int ezbobNumOfLateRepayments, bool firstRepaymentDatePassed)
		{
			return GetBaseWeight(ezbobNumOfLateRepayments, NumOfLateRepaymentsBaseWeight, MedalRangesConstats.NumOfLateRepaymentsRanges,
				firstRepaymentDatePassed, NumOfLateRepaymentsFirstRepaymentWeight);
		}

		protected virtual Weight GetNumOfOnTimeLoansWeight(int ezbobNumOfLoans, bool firstRepaymentDatePassed)
		{
			return GetBaseWeight(ezbobNumOfLoans, NumOfOnTimeLoansBaseWeight, MedalRangesConstats.NumOfOnTimeLoansRanges,
				firstRepaymentDatePassed, NumOfOnTimeLoansFirstRepaymentWeight);
		}

		protected virtual Weight GetEzbobSeniorityWeight(decimal ezbobSeniority, bool firstRepaymentDatePassed)
		{
			return GetBaseWeight(ezbobSeniority, EzbobSeniorityBaseWeight, MedalRangesConstats.EzbobSeniorityRanges,
				firstRepaymentDatePassed, EzbobSeniorityFirstRepaymentWeight);
		}

		protected virtual Weight GetPositiveFeedbacksWeight(int positiveFeedbacks)
		{
			return GetBaseWeight(positiveFeedbacks, PositiveFeedbacksBaseWeight, MedalRangesConstats.PositiveFeedbacksRanges);
		}

		protected virtual Weight GetNumOfStoresWeight(int numOfStores)
		{
			return GetBaseWeight(numOfStores, NumOfStoresBaseWeight, MedalRangesConstats.NumOfStoresRanges);
		}

		protected virtual Weight GetAnnualTurnoverWeight(decimal annualTurnover, bool useHmrc)
		{
			var annualTurnoverWeight = GetBaseWeight(annualTurnover, AnnualTurnoverBaseWeight, MedalRangesConstats.AnnualTurnoverRanges);

			if (!useHmrc)
			{
				annualTurnoverWeight.FinalWeight += AnnualTurnoverNoHmrcWeightChange;
			}

			return annualTurnoverWeight;
		}

		protected virtual Weight GetMaritalStatusWeight(MaritalStatus maritalStatus)
		{
			var maritalStatusWeight = new Weight
			{
				Value = maritalStatus.ToString(),
				FinalWeight = MaritalStatusBaseWeight,
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

		protected virtual Weight GetNetWorthWeight(decimal netWorth, bool firstRepaymentDatePassed)
		{
			var netWorthWeight = GetBaseWeight(netWorth, NetWorthBaseWeight, MedalRangesConstats.NetWorthRanges);
			if (firstRepaymentDatePassed) {
				netWorthWeight.FinalWeight += NetWorthFirstRepaymentWeightChange;
			}
			return netWorthWeight;
		}

		protected virtual Weight GetFreeCashFlowWeight(decimal freeCashFlow, bool useHmrc, decimal annualTurnover)
		{
			var freeCashFlowWeight = GetBaseWeight(freeCashFlow, FreeCashFlowBaseWeight, MedalRangesConstats.FreeCashFlowRanges);

			if (!useHmrc)
			{
				freeCashFlowWeight.FinalWeight = FreeCashFlowNoHmrcWeight;
				freeCashFlowWeight.Grade = MedalRangesConstats.FreeCashFlowRanges.MinGrade();
			}

			if (annualTurnover == 0)
			{
				freeCashFlowWeight.Grade = MedalRangesConstats.FreeCashFlowRanges.MinGrade();
			}

			return freeCashFlowWeight;
		}

		protected virtual Weight GetBusinessSeniorityWeight(decimal businessSeniority, bool firstRepaymentDatePassed, bool useHmrc)
		{
			var businessSeniorityWeight = GetBaseWeight(businessSeniority, BusinessSeniorityBaseWeight, MedalRangesConstats.BusinessSeniorityRanges);

			if (!useHmrc)
			{
				businessSeniorityWeight.FinalWeight += BusinessSeniorityNoHmrcWeightChange;
			}

			if (firstRepaymentDatePassed)
			{
				businessSeniorityWeight.FinalWeight += BusinessSeniorityFirstRepaymentWeightChange;
			}

			return businessSeniorityWeight;
		}

		protected virtual Weight GetTangibleEquityWeight(decimal tangibleEquity, decimal annualTurnover)
		{
			var tangibleEquityWeight = GetBaseWeight(tangibleEquity, TangibleEquityBaseWeight, MedalRangesConstats.TangibleEquityRanges);
			if (annualTurnover == 0) {
				tangibleEquityWeight.Grade = MedalRangesConstats.TangibleEquityRanges.MinGrade();
			}
			return tangibleEquityWeight;
		}

		protected virtual Weight GetBusinessScoreWeight(int businessScore, bool firstRepaymentDatePassed, bool useHmrc)
		{
			var businessScoreWeight = GetBaseWeight(businessScore, BusinessScoreBaseWeight, MedalRangesConstats.BusinessScoreRanges);

			if (businessScore <= LowBusinessScore)
			{
				businessScoreWeight.FinalWeight = BusinessScoreLowScoreWeight;
			}

			if (!useHmrc)
			{
				businessScoreWeight.FinalWeight += BusinessScoreNoHmrcWeightChange;
			}

			if (firstRepaymentDatePassed)
			{
				businessScoreWeight.FinalWeight += BusinessScoreFirstRepaymentWeightChange;
			}

			return businessScoreWeight;
		}

		protected virtual Weight GetConsumerScoreWeight(int consumerScore, bool firstRepaymentDatePassed, bool useHmrc)
		{
			var consumerScoreWeight = GetBaseWeight(consumerScore, ConsumerScoreBaseWeight, MedalRangesConstats.ConsumerScoreRanges);

			if (consumerScore <= LowConsumerScore)
			{
				consumerScoreWeight.FinalWeight = ConsumerScoreLowScoreWeight;
			}

			if (!useHmrc)
			{
				consumerScoreWeight.FinalWeight += ConsumerScoreNoHmrcWeightChange;
			}

			if (firstRepaymentDatePassed)
			{
				consumerScoreWeight.FinalWeight += ConsumerScoreFirstRepaymentWeightChange;
			}

			return consumerScoreWeight;
		}
		#endregion


		#region Weights
		public abstract decimal BusinessScoreBaseWeight { get; }
		public abstract decimal FreeCashFlowBaseWeight { get; }
		public abstract decimal AnnualTurnoverBaseWeight { get; }
		public virtual decimal TangibleEquityBaseWeight { get { return 0; } } //only for limited
		public abstract decimal BusinessSeniorityBaseWeight { get; }
		public abstract decimal ConsumerScoreBaseWeight { get; }
		public abstract decimal NetWorthBaseWeight { get; }
		public abstract decimal MaritalStatusBaseWeight { get; }
		public virtual decimal NumOfStoresBaseWeight { get { return 0; } } //only for online
		public virtual decimal PositiveFeedbacksBaseWeight { get { return 0; } } //only for online
		public virtual decimal EzbobSeniorityBaseWeight { get { return 0; } } //common
		public virtual decimal NumOfOnTimeLoansBaseWeight { get { return 0; } } //common
		public virtual decimal NumOfLateRepaymentsBaseWeight { get { return 0; } } //common
		public virtual decimal NumOfEarlyRepaymentsBaseWeight { get { return 0; } } //common
		public abstract decimal FreeCashFlowNoHmrcWeight { get; }
		public abstract decimal AnnualTurnoverNoHmrcWeightChange { get; }
		public abstract decimal BusinessScoreNoHmrcWeightChange { get; }
		public abstract decimal ConsumerScoreNoHmrcWeightChange { get; }
		public abstract decimal BusinessSeniorityNoHmrcWeightChange { get; }
		public virtual int LowBusinessScore { get { return 30; } } //common
		public virtual int LowConsumerScore { get { return 800; } } //common
		public abstract decimal BusinessScoreLowScoreWeight { get; }
		public abstract decimal ConsumerScoreLowScoreWeight { get; }

		public virtual decimal EzbobSeniorityFirstRepaymentWeight { get { return 0.02M; } }
		public virtual decimal NumOfOnTimeLoansFirstRepaymentWeight { get { return 0.0333M; } }
		public virtual decimal NumOfLateRepaymentsFirstRepaymentWeight { get { return 0.0267M; } }
		public virtual decimal NumOfEarlyRepaymentsFirstRepaymentWeight { get { return 0.02M; } }
		
		public abstract decimal ConsumerScoreFirstRepaymentWeightChange { get; }
		public abstract decimal BusinessScoreFirstRepaymentWeightChange { get; }
		public abstract decimal BusinessSeniorityFirstRepaymentWeightChange { get; }
		public virtual decimal NetWorthFirstRepaymentWeightChange { get { return 0; } }
		#endregion
	}
}
