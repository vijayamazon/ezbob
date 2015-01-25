namespace AutomationCalculator.MedalCalculation {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using AutomationCalculator.Common;
	using AutomationCalculator.Turnover;
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
	} // interface IMedalWeightConstatns

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

			// --------start new turnover calculation for medal ----------------//
			//	Flow: https://drive.draw.io/?#G0B1Io_qu9i44ScEJqeUlLNEhaa28
			model.TurnoverType = null;
			model.Turnovers = new List<TurnoverDbRow>();

			this.DB.ForEachResult<TurnoverDbRow>(r => model.Turnovers.Add(r),
					"GetCustomerTurnoverForAutoDecision",
					new QueryParameter("IsForApprove", true),
					new QueryParameter("CustomerID", customerId),
					new QueryParameter("Now", model.CalculationDate));

			//foreach (var t in turnovers)  Console.WriteLine("Allturnovers: {0}, {1}, {2}", t.TheMonth, t.Distance, t.MpTypeID);

			// extract hmrc data 
			var hmrcList = (from TurnoverDbRow r in model.Turnovers where r.MpTypeID.Equals(TurnoverDbRow.hmrc) select r).AsQueryable();
			// extract yodlee data only
			var yodleeList = (from TurnoverDbRow r in model.Turnovers where r.MpTypeID.Equals(TurnoverDbRow.yodlee) select r).AsQueryable();

			decimal hmrcTurnover = 0;
			decimal yoodleeTurnover = 0;

			// has a hmrc
			if (Enumerable.Count(hmrcList) > 0) {
				// get hmrc turnover for all months received
				hmrcTurnover = hmrcList.Sum(t => t.Turnover);
				model.AnnualTurnover = (hmrcTurnover < 0) ? 0 : hmrcTurnover;
				model.HmrcAnnualTurnover = model.AnnualTurnover;
				model.TurnoverType = TurnoverType.HMRC;
			}

			// has bank instead
			if (model.TurnoverType.Equals(null)) {
				if (Enumerable.Count(yodleeList) > 0) {
					// get yoodlee turnover for all months received
					yoodleeTurnover = yodleeList.Sum(t => t.Turnover);
					model.AnnualTurnover = (yoodleeTurnover < 0) ? 0 : yoodleeTurnover;
					model.YodleeAnnualTurnover = model.AnnualTurnover;
					model.TurnoverType = TurnoverType.Bank;
				}
			}

			this.Log.Info("AV: AnnualTurnover: {0}, type:{1}", model.AnnualTurnover, model.TurnoverType);

			// --------end new turnover calculation for medal----------------//

			// TODO: free cash flow value, value added
			model.FreeCashFlowValue = dbData.HmrcFreeCashFlow - (dbData.CurrentBalanceSum / dbData.FCFFactor);
			model.ValueAdded = dbData.HmrcValueAdded;
			// end of TODO

			model.FreeCashFlow = model.AnnualTurnover == 0 || !dbData.HasHmrc ? 0 : model.FreeCashFlowValue / model.AnnualTurnover;
			model.TangibleEquity = model.AnnualTurnover == 0 ? 0 : model.MedalInputModelDb.TangibleEquity / model.AnnualTurnover;
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

			var onlineTurnover = GetOnlineTurnover(intputModel.Turnovers);

			if (intputModel.MedalInputModelDb.HmrcRevenues > onlineTurnover * intputModel.MedalInputModelDb.OnlineMedalTurnoverCutoff) {
				usingHmrc = true;
				intputModel.AnnualTurnover = intputModel.MedalInputModelDb.HmrcRevenues;
				intputModel.TurnoverType = TurnoverType.HMRC;
			} else if (intputModel.YodleeAnnualTurnover > onlineTurnover * intputModel.MedalInputModelDb.OnlineMedalTurnoverCutoff) {
				intputModel.AnnualTurnover = intputModel.YodleeAnnualTurnover;
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

			intputModel.PositiveFeedbacks = new MarketPlacesHelper(DB, Log).GetPositiveFeedbacks(customerId);
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


		protected virtual decimal GetOnlineTurnover(List<TurnoverDbRow> turnovers) {
			try {
				const int T1 = 1;
				const int T3 = 3;
				const int T6 = 6;
				const int T12 = 12;

				const int Ec1 = 12;
				const int Ec3 = 4;
				const int Ec6 = 2;
				const int Ec12 = 1;

				List<decimal> list_t1 = new List<decimal>();
				List<decimal> list_t3 = new List<decimal>();
				List<decimal> list_t6 = new List<decimal>();
				List<decimal> list_t12 = new List<decimal>();

				// extact amazon data
				var amazonList = (from TurnoverDbRow r in turnovers where r.MpTypeID.Equals(TurnoverDbRow.amazon) select r).AsQueryable();

				//		foreach (var y in amazonList) Log.Info("AmazonList: Distance: {0}, TheMonth: {1}, Turnover: {2}, CurrentMonth: {3}", y.Distance, y.TheMonth, y.Turnover, y.CurrentMonth);

				// calculate "last month", "last three months", "last six months", and "last twelve months"/annualize for amazon
				list_t1.Add(CalcAnnualTurnoverBasedOnPartialData(amazonList, T1, Ec1));
				list_t3.Add(CalcAnnualTurnoverBasedOnPartialData(amazonList, T3, Ec3));
				list_t6.Add(CalcAnnualTurnoverBasedOnPartialData(amazonList, T6, Ec6));
				list_t12.Add(CalcAnnualTurnoverBasedOnPartialData(amazonList, T12, Ec12));

				//		Log.Info("Amazon TT: t1: {0}, t3: {1}, t6: {2}, t12: {3}", list_t1[0], list_t3[0], list_t6[0], list_t12[0]);

				// extact ebay data
				var ebayList = (from TurnoverDbRow r in turnovers where r.MpTypeID.Equals(TurnoverDbRow.ebay) select r).AsQueryable();

				//foreach (var y in ebayList) Log.Info("ebayList: Distance: {0}, TheMonth: {1}, Turnover: {2}, CurrentMonth: {3}", y.Distance, y.TheMonth, y.Turnover, y.CurrentMonth);

				// calculate "last month", "last three months", "last six months", and "last twelve months"/annualize for ebay
				list_t1.Add(CalcAnnualTurnoverBasedOnPartialData(ebayList, T1, Ec1));
				list_t3.Add(CalcAnnualTurnoverBasedOnPartialData(ebayList, T3, Ec3));
				list_t6.Add(CalcAnnualTurnoverBasedOnPartialData(ebayList, T6, Ec6));
				list_t12.Add(CalcAnnualTurnoverBasedOnPartialData(ebayList, T12, Ec12));

				//		Log.Info("Ebay TT: t1: {0}, t3: {1}, t6: {2}, t12: {3}", list_t1[1], list_t3[1], list_t6[1], list_t12[1]);

				// extact paypal data
				var paypalList = (from TurnoverDbRow r in turnovers where r.MpTypeID.Equals(TurnoverDbRow.paypal) select r).AsQueryable();

				//		foreach (var y in paypalList) Log.Info("paypalList: Distance: {0}, TheMonth: {1}, Turnover: {2}, CurrentMonth: {3}", y.Distance, y.TheMonth, y.Turnover, y.CurrentMonth);

				// calculate "last month", "last three months", "last six months", and "last twelve months"/annualize for paypal
				list_t1.Add(CalcAnnualTurnoverBasedOnPartialData(paypalList, T1, Ec1));
				list_t3.Add(CalcAnnualTurnoverBasedOnPartialData(paypalList, T3, Ec3));
				list_t6.Add(CalcAnnualTurnoverBasedOnPartialData(paypalList, T6, Ec6));
				list_t12.Add(CalcAnnualTurnoverBasedOnPartialData(paypalList, T12, Ec12));

				//	Log.Info("Paypal TT: t1: {0}, t3: {1}, t6: {2}, t12: {3}", list_t1[2], list_t3[2], list_t6[2], list_t12[2]);

				// Online turnover: Amazon + MAX(eBay, Pay Pal)

				// amazon: index 0
				// ebay: index 1
				// paypal: index 2

				var onlineList = new ArrayList();

				onlineList.Add(list_t1.ElementAt(0) + Math.Max(list_t1.ElementAt(1), list_t1.ElementAt(2)));
				onlineList.Add(list_t3.ElementAt(0) + Math.Max(list_t3.ElementAt(1), list_t3.ElementAt(2)));
				onlineList.Add(list_t6.ElementAt(0) + Math.Max(list_t6.ElementAt(1), list_t6.ElementAt(2)));
				onlineList.Add(list_t12.ElementAt(0) + Math.Max(list_t12.ElementAt(1), list_t12.ElementAt(2)));

				decimal annualTurnover = (from decimal r in onlineList where r > 0 select r).AsQueryable().DefaultIfEmpty(0).Min();

				return annualTurnover < 0 ? 0 : annualTurnover;
			} catch (Exception ex) {
				this.Log.Error(ex, "Failed to calculate online annual turnover for medal");
				return 0;
			} // try
		} // CalculateTurnoverForMedal


		/// <summary>
		///     Calculates for "last month", "last three months", "last six months", and "last twelve months".
		///     Annualize the figures and take the minimum among them.
		/// </summary>
		/// <param name="list"></param>
		/// <param name="monthsBefore"></param>
		/// <param name="extrapolationCoefficient"></param>
		/// <returns></returns>
		protected virtual decimal CalcAnnualTurnoverBasedOnPartialData(IQueryable<TurnoverDbRow> list, int monthAfter, int extrapolationCoefficient) {
			return list.Where(t => (t.Distance < monthAfter)).Sum(t => t.Turnover) * extrapolationCoefficient;
		}


		protected readonly ASafeLog Log;
		protected readonly AConnection DB;
	}
}
