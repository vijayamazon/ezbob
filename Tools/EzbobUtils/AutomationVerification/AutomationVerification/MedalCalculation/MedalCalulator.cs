namespace AutomationCalculator.MedalCalculation {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using AutomationCalculator.Common;
	using Ezbob.Database;
	using Ezbob.Logger;

	/// <summary>
	///     Medal calculator interface.
	/// </summary>
	public interface IMedalCalulator {
		/// <summary>
		///     Used to retrieve customer's input parameters for medal calculation
		/// </summary>
		/// <param name="customerId">Customer Id</param>
		/// <param name="calculationDate">optional for historical data</param>
		/// <returns>the medal input model</returns>
		MedalInputModel GetInputParameters(int customerId, DateTime? calculationDate = null);

		/// <summary>
		///     Calculates the medal score, medal classification
		/// </summary>
		/// <param name="model">Medal input model</param>
		/// <returns>medal output model</returns>
		MedalOutputModel CalculateMedal(MedalInputModel model);
	} // interface IMedalCalulator

	/// <summary>
	///     Medal weight constants interface containing all the medal weight constants overridden in each medal
	/// </summary>
	public interface IMedalWeightConstatns {
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

		decimal FreeCashFlowNoHmrcWeight { get; }
		decimal AnnualTurnoverNoHmrcWeightChange { get; }
		decimal BusinessScoreNoHmrcWeightChange { get; }
		decimal ConsumerScoreNoHmrcWeightChange { get; }
		decimal BusinessSeniorityNoHmrcWeightChange { get; }

		int LowBusinessScore { get; }
		int LowConsumerScore { get; }

		decimal BusinessScoreLowScoreWeight { get; }
		decimal ConsumerScoreLowScoreWeight { get; }

		decimal EzbobSeniorityFirstRepaymentWeight { get; }
		decimal NumOfOnTimeLoansFirstRepaymentWeight { get; }
		decimal NumOfLateRepaymentsFirstRepaymentWeight { get; }
		decimal NumOfEarlyRepaymentsFirstRepaymentWeight { get; }

		decimal ConsumerScoreFirstRepaymentWeightChange { get; }
		decimal BusinessScoreFirstRepaymentWeightChange { get; }
		decimal BusinessSeniorityFirstRepaymentWeightChange { get; }
		decimal NetWorthFirstRepaymentWeightChange { get; }
	}

	/// <summary>
	///     Base medal calculator abstract class implements the 2 interfaces and the common for all medals method for weight
	///     calculation
	/// </summary>
	public abstract class MedalCalculator : IMedalCalulator, IMedalWeightConstatns {
		public abstract decimal BusinessScoreBaseWeight { get; }

		public abstract decimal FreeCashFlowBaseWeight { get; }

		public abstract decimal AnnualTurnoverBaseWeight { get; }

		public virtual decimal TangibleEquityBaseWeight {
			get { return 0; }
		}

		//only for limited
		public abstract decimal BusinessSeniorityBaseWeight { get; }

		public abstract decimal ConsumerScoreBaseWeight { get; }

		public abstract decimal NetWorthBaseWeight { get; }

		public abstract decimal MaritalStatusBaseWeight { get; }

		public virtual decimal NumOfStoresBaseWeight {
			get { return 0; }
		}

		//only for online
		public virtual decimal PositiveFeedbacksBaseWeight {
			get { return 0; }
		}

		//only for online
		public virtual decimal EzbobSeniorityBaseWeight {
			get { return 0; }
		}

		//common
		public virtual decimal NumOfOnTimeLoansBaseWeight {
			get { return 0; }
		}

		//common
		public virtual decimal NumOfLateRepaymentsBaseWeight {
			get { return 0; }
		}

		//common
		public virtual decimal NumOfEarlyRepaymentsBaseWeight {
			get { return 0; }
		}

		//common
		public abstract decimal FreeCashFlowNoHmrcWeight { get; }

		public abstract decimal AnnualTurnoverNoHmrcWeightChange { get; }

		public abstract decimal BusinessScoreNoHmrcWeightChange { get; }

		public abstract decimal ConsumerScoreNoHmrcWeightChange { get; }

		public abstract decimal BusinessSeniorityNoHmrcWeightChange { get; }

		public virtual int LowBusinessScore {
			get { return 30; }
		}

		//common
		public virtual int LowConsumerScore {
			get { return 800; }
		}

		//common
		public abstract decimal BusinessScoreLowScoreWeight { get; }

		public abstract decimal ConsumerScoreLowScoreWeight { get; }

		public virtual decimal EzbobSeniorityFirstRepaymentWeight {
			get { return 0.02M; }
		}

		public virtual decimal NumOfOnTimeLoansFirstRepaymentWeight {
			get { return 0.0333M; }
		}

		public virtual decimal NumOfLateRepaymentsFirstRepaymentWeight {
			get { return 0.0267M; }
		}

		public virtual decimal NumOfEarlyRepaymentsFirstRepaymentWeight {
			get { return 0.02M; }
		}

		public abstract decimal ConsumerScoreFirstRepaymentWeightChange { get; }

		public abstract decimal BusinessScoreFirstRepaymentWeightChange { get; }

		public abstract decimal BusinessSeniorityFirstRepaymentWeightChange { get; }

		public virtual decimal NetWorthFirstRepaymentWeightChange {
			get { return 0; }
		}

		public virtual MedalInputModel GetInputParameters(int customerId, DateTime? calculationDate = null) {
			var dbHelper = new DbHelper(this.DB, this.Log);
			var dbData = dbHelper.GetMedalInputModel(customerId);
			var model = new MedalInputModel {
				MedalInputModelDb = dbData
			};

			var today = calculationDate.HasValue ? calculationDate.Value : DateTime.Today;
			model.CalculationDate = today;

			model.HasHmrc = dbData.HasHmrc;
			model.UseHmrc = dbData.HasHmrc;
			model.BusinessScore = dbData.BusinessScore;

			const int defaultBusinessSeniority = 1;
			model.BusinessSeniority = dbData.IncorporationDate.HasValue ? today.Year - dbData.IncorporationDate.Value.Year : defaultBusinessSeniority;
			if (dbData.IncorporationDate.HasValue && dbData.IncorporationDate.Value > today.AddYears(-model.BusinessSeniority))
				model.BusinessSeniority--;

			model.ConsumerScore = dbData.ConsumerScore;
			model.EzbobSeniority = (decimal)(today - dbData.RegistrationDate).TotalDays / (365.0M / 12.0M);
			model.FirstRepaymentDatePassed = dbData.FirstRepaymentDate.HasValue && dbData.FirstRepaymentDate.Value < today;
			model.NumOfEarlyPayments = dbData.NumOfEarlyPayments;
			model.NumOfLatePayments = dbData.NumOfLatePayments;
			model.NumOfOnTimeLoans = dbData.NumOfOnTimeLoans;
			model.MaritalStatus = (MaritalStatus)Enum.Parse(typeof(MaritalStatus), dbData.MaritalStatus);
			model.NetWorth = dbData.ZooplaValue == 0 ? 0 : (dbData.ZooplaValue - dbData.Mortages) / (decimal)dbData.ZooplaValue;

			// This Logic is good only for OfflineLimited,SoleTrader,NonLimited Medals
			List<MarketPlace> yodlees = dbHelper.GetCustomerYodlees(customerId);
			var mpHelper = new MarketPlacesHelper(this.DB, this.Log);
			var yodleeIncome = mpHelper.GetYodleeAnnualized(yodlees, this.Log);
			model.AnnualTurnover = dbData.HasHmrc ? dbData.HmrcRevenues : yodleeIncome;
			model.AnnualTurnover = model.AnnualTurnover < 0 ? 0 : model.AnnualTurnover;

			if (dbData.HasHmrc)
				model.TurnoverType = TurnoverType.HMRC;
			else if (yodlees.Count > 0)
				model.TurnoverType = TurnoverType.Bank;
			else
				model.TurnoverType = null;

			model.FreeCashFlowValue = dbData.HmrcFreeCashFlow - (dbData.CurrentBalanceSum / dbData.FCFFactor);
			model.FreeCashFlow = model.AnnualTurnover == 0 || !dbData.HasHmrc ? 0 : model.FreeCashFlowValue / model.AnnualTurnover;
			model.TangibleEquity = model.AnnualTurnover == 0 ? 0 : model.MedalInputModelDb.TangibleEquity / model.AnnualTurnover;
			model.ValueAdded = dbData.HmrcValueAdded;
			model.CustomerId = customerId;
			return model;
		}

		public abstract MedalOutputModel CalculateMedal(MedalInputModel model);

		protected MedalCalculator(AConnection db, ASafeLog log) {
			this.DB = db;
			this.Log = log;
		}

		/// <summary>
		///     Delta calculation is used to redistribute weights in case customer have low score
		/// </summary>
		/// <param name="model"></param>
		/// <param name="dict"></param>
		protected abstract void CalcDelta(MedalInputModel model, Dictionary<Parameter, Weight> dict);

		/// <summary>
		///     If HMRC turnover > 0.7*OnlineTurnover use HMRC turnover, otherwise, if yodlee turnover > 0.7*OnlineTurnover use
		///     yodlee turnover, otherwise use online turnover
		///     If it is negative – use 0 instead
		///     The 0.7 should be configurable via UW – config name: OnlineMedalTurnoverCutoff
		/// </summary>
		protected MedalInputModel GetOnlineInputParameters(int customerId, MedalInputModel intputModel) {
			var usingHmrc = false;
			var dbHelper = new DbHelper(this.DB, this.Log);
			var mpHelper = new MarketPlacesHelper(this.DB, this.Log);
			var yodlees = dbHelper.GetCustomerYodlees(customerId);
			var yodleeIncome = mpHelper.GetYodleeAnnualized(yodlees, this.Log);
			var onlineTurnover = mpHelper.GetOnlineTurnoverAnnualized(customerId);
			if (intputModel.MedalInputModelDb.HmrcRevenues > onlineTurnover * intputModel.MedalInputModelDb.OnlineMedalTurnoverCutoff) {
				usingHmrc = true;
				intputModel.AnnualTurnover = intputModel.MedalInputModelDb.HmrcRevenues;
				intputModel.TurnoverType = TurnoverType.HMRC;
			} else if (yodleeIncome > onlineTurnover * intputModel.MedalInputModelDb.OnlineMedalTurnoverCutoff) {
				intputModel.AnnualTurnover = yodleeIncome;
				intputModel.TurnoverType = TurnoverType.Bank;
			} else {
				intputModel.AnnualTurnover = onlineTurnover;
				intputModel.TurnoverType = TurnoverType.Online;
			}

			if (onlineTurnover < 0)
				intputModel.AnnualTurnover = 0;

			intputModel.FreeCashFlow = intputModel.AnnualTurnover == 0 || !intputModel.HasHmrc ? 0 : intputModel.FreeCashFlowValue / intputModel.AnnualTurnover;

			intputModel.TangibleEquity = intputModel.AnnualTurnover == 0 ? 0 : intputModel.MedalInputModelDb.TangibleEquity / intputModel.AnnualTurnover;
			intputModel.NumOfStores = intputModel.MedalInputModelDb.NumOfStores;

			intputModel.PositiveFeedbacks = mpHelper.GetPositiveFeedbacks(customerId);
			intputModel.UseHmrc = usingHmrc;

			return intputModel;
		}

		/// <summary>
		///     Score is calculated by sum of all Weight*Grade for each parameter
		///     The normalized score is calculated by (scoreSum - minScoreSum) / (maxScoreSum - minScoreSum)
		/// </summary>
		/// <param name="dict">Weight dictionary</param>
		/// <param name="inputModel">Medal input model</param>
		/// <param name="medalType">Medal type that is calculated</param>
		/// <returns>Medal output model</returns>
		protected MedalOutputModel CalcScoreMedalOffer(Dictionary<Parameter, Weight> dict, MedalInputModel inputModel, MedalType medalType = MedalType.NoMedal) {
			decimal minScoreSum = 0;
			decimal maxScoreSum = 0;
			decimal scoreSum = 0;
			foreach (var weight in dict.Values) {
				weight.MinimumScore = weight.FinalWeight * weight.MinimumGrade;
				weight.MaximumScore = weight.FinalWeight * weight.MaximumGrade;
				weight.Score = weight.Grade * weight.FinalWeight;
				minScoreSum += weight.MinimumScore;
				maxScoreSum += weight.MaximumScore;
				scoreSum += weight.Score;
			}

			decimal score = (scoreSum - minScoreSum) / (maxScoreSum - minScoreSum);
			Medal medal = GetMedal(MedalRangesConstats.MedalRanges, score);
			var medalOutput = new MedalOutputModel {
				TurnoverType = inputModel.TurnoverType,
				Medal = medal,
				MedalType = medalType,
				NormalizedScore = score,
				Score = scoreSum,
				WeightsDict = dict,
				AnnualTurnover = inputModel.AnnualTurnover,
				FreeCashflow = inputModel.FreeCashFlowValue,
				FirstRepaymentDatePassed = inputModel.FirstRepaymentDatePassed,
				ValueAdded = inputModel.ValueAdded,
				CustomerId = inputModel.CustomerId,
				CalculationDate = inputModel.CalculationDate,
				UseHmrc = inputModel.UseHmrc,
			};
			return medalOutput;
		}

		protected Weight GetBaseWeight(decimal value, decimal baseWeight, List<RangeGrage> ranges, bool firstRepaymentDatePassed = false, decimal firstRepaymentWeight = 0) {
			var weight = new Weight {
				Value = value.ToString(CultureInfo.InvariantCulture),
				FinalWeight = baseWeight,
				MinimumGrade = ranges.MinGrade(),
				MaximumGrade = ranges.MaxGrade(),
				Grade = GetGrade(ranges, value),
			};

			if (firstRepaymentDatePassed)
				weight.FinalWeight = firstRepaymentWeight;

			return weight;
		}

		protected Medal GetMedal(IEnumerable<RangeMedal> rangeMedals, decimal value) {
			var range = rangeMedals.GetRange(value);
			if (range != null)
				return range.Medal;
			return Medal.NoClassification;
		}

		protected int GetGrade(IEnumerable<RangeGrage> rangeGrages, decimal value) {
			var range = rangeGrages.GetRange(value);
			if (range != null)
				return range.Grade;
			return 0;
		}

		protected virtual Weight GetNumOfEarlyPaymentsWeight(int ezbobNumOfEarlyReayments, bool firstRepaymentDatePassed) {
			return GetBaseWeight(ezbobNumOfEarlyReayments, NumOfEarlyRepaymentsBaseWeight, MedalRangesConstats.NumOfEarlyRepaymentsRanges,
				firstRepaymentDatePassed, NumOfEarlyRepaymentsFirstRepaymentWeight);
		}

		protected virtual Weight GetNumOfLatePaymentsWeight(int ezbobNumOfLateRepayments, bool firstRepaymentDatePassed) {
			return GetBaseWeight(ezbobNumOfLateRepayments, NumOfLateRepaymentsBaseWeight, MedalRangesConstats.NumOfLateRepaymentsRanges,
				firstRepaymentDatePassed, NumOfLateRepaymentsFirstRepaymentWeight);
		}

		protected virtual Weight GetNumOfOnTimeLoansWeight(int ezbobNumOfLoans, bool firstRepaymentDatePassed) {
			return GetBaseWeight(ezbobNumOfLoans, NumOfOnTimeLoansBaseWeight, MedalRangesConstats.NumOfOnTimeLoansRanges,
				firstRepaymentDatePassed, NumOfOnTimeLoansFirstRepaymentWeight);
		}

		protected virtual Weight GetEzbobSeniorityWeight(decimal ezbobSeniority, bool firstRepaymentDatePassed) {
			return GetBaseWeight(ezbobSeniority, EzbobSeniorityBaseWeight, MedalRangesConstats.EzbobSeniorityRanges,
				firstRepaymentDatePassed, EzbobSeniorityFirstRepaymentWeight);
		}

		protected virtual Weight GetPositiveFeedbacksWeight(int positiveFeedbacks) {
			return GetBaseWeight(positiveFeedbacks, PositiveFeedbacksBaseWeight, MedalRangesConstats.PositiveFeedbacksRanges);
		}

		protected virtual Weight GetNumOfStoresWeight(int numOfStores) {
			return GetBaseWeight(numOfStores, NumOfStoresBaseWeight, MedalRangesConstats.NumOfStoresRanges);
		}

		protected virtual Weight GetAnnualTurnoverWeight(decimal annualTurnover, bool useHmrc) {
			var annualTurnoverWeight = GetBaseWeight(annualTurnover, AnnualTurnoverBaseWeight, MedalRangesConstats.AnnualTurnoverRanges);

			if (!useHmrc)
				annualTurnoverWeight.FinalWeight += AnnualTurnoverNoHmrcWeightChange;

			return annualTurnoverWeight;
		}

		protected virtual Weight GetMaritalStatusWeight(MaritalStatus maritalStatus) {
			var maritalStatusWeight = new Weight {
				Value = maritalStatus.ToString(),
				FinalWeight = MaritalStatusBaseWeight,
				MinimumGrade = MedalRangesConstats.MaritalStatusGrade_Single,
				MaximumGrade = MedalRangesConstats.MaritalStatusGrade_Married
			};

			switch (maritalStatus) {
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

		protected virtual Weight GetNetWorthWeight(decimal netWorth, bool firstRepaymentDatePassed) {
			var netWorthWeight = GetBaseWeight(netWorth, NetWorthBaseWeight, MedalRangesConstats.NetWorthRanges);
			if (firstRepaymentDatePassed)
				netWorthWeight.FinalWeight += NetWorthFirstRepaymentWeightChange;
			return netWorthWeight;
		}

		protected virtual Weight GetFreeCashFlowWeight(decimal freeCashFlow, bool useHmrc, decimal annualTurnover) {
			var freeCashFlowWeight = GetBaseWeight(freeCashFlow, FreeCashFlowBaseWeight, MedalRangesConstats.FreeCashFlowRanges);

			if (!useHmrc) {
				freeCashFlowWeight.FinalWeight = FreeCashFlowNoHmrcWeight;
				freeCashFlowWeight.Grade = MedalRangesConstats.FreeCashFlowRanges.MinGrade();
			}

			if (annualTurnover == 0)
				freeCashFlowWeight.Grade = MedalRangesConstats.FreeCashFlowRanges.MinGrade();

			return freeCashFlowWeight;
		}

		protected virtual Weight GetBusinessSeniorityWeight(decimal businessSeniority, bool firstRepaymentDatePassed, bool useHmrc) {
			var businessSeniorityWeight = GetBaseWeight(businessSeniority, BusinessSeniorityBaseWeight, MedalRangesConstats.BusinessSeniorityRanges);

			if (!useHmrc)
				businessSeniorityWeight.FinalWeight += BusinessSeniorityNoHmrcWeightChange;

			if (firstRepaymentDatePassed)
				businessSeniorityWeight.FinalWeight += BusinessSeniorityFirstRepaymentWeightChange;

			return businessSeniorityWeight;
		}

		protected virtual Weight GetTangibleEquityWeight(decimal tangibleEquity, decimal annualTurnover) {
			var tangibleEquityWeight = GetBaseWeight(tangibleEquity, TangibleEquityBaseWeight, MedalRangesConstats.TangibleEquityRanges);
			if (annualTurnover == 0)
				tangibleEquityWeight.Grade = MedalRangesConstats.TangibleEquityRanges.MinGrade();
			return tangibleEquityWeight;
		}

		protected virtual Weight GetBusinessScoreWeight(int businessScore, bool firstRepaymentDatePassed, bool useHmrc) {
			var businessScoreWeight = GetBaseWeight(businessScore, BusinessScoreBaseWeight, MedalRangesConstats.BusinessScoreRanges);

			if (businessScore <= LowBusinessScore)
				businessScoreWeight.FinalWeight = BusinessScoreLowScoreWeight;

			if (!useHmrc)
				businessScoreWeight.FinalWeight += BusinessScoreNoHmrcWeightChange;

			if (firstRepaymentDatePassed)
				businessScoreWeight.FinalWeight += BusinessScoreFirstRepaymentWeightChange;

			return businessScoreWeight;
		}

		protected virtual Weight GetConsumerScoreWeight(int consumerScore, bool firstRepaymentDatePassed, bool useHmrc) {
			var consumerScoreWeight = GetBaseWeight(consumerScore, ConsumerScoreBaseWeight, MedalRangesConstats.ConsumerScoreRanges);

			if (consumerScore <= LowConsumerScore)
				consumerScoreWeight.FinalWeight = ConsumerScoreLowScoreWeight;

			if (!useHmrc)
				consumerScoreWeight.FinalWeight += ConsumerScoreNoHmrcWeightChange;

			if (firstRepaymentDatePassed)
				consumerScoreWeight.FinalWeight += ConsumerScoreFirstRepaymentWeightChange;

			return consumerScoreWeight;
		}

		protected readonly ASafeLog Log;
		protected readonly AConnection DB;
	}
}
