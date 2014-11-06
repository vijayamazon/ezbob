namespace AutomationCalculator.MedalCalculation
{
	using System.Collections.Generic;
	using System.Linq;
	using Common;
	using Ezbob.Logger;

	public class OnlineLImitedMedalCalculator : MedalCalculator
	{
		public OnlineLImitedMedalCalculator(ASafeLog log) : base(log) { }

		public override MedalInputModel GetInputParameters(int customerId) {
			var model = base.GetInputParameters(customerId);
			if (model.HasMoreThanOneHmrc) {
				return model;
			}
			model.AnnualTurnover = GetOnlineAnnualTurnover(customerId, model.MedalInputModelDb);
			var balance = model.MedalInputModelDb.CurrentBalanceSum < 0 ? 0 : (model.MedalInputModelDb.CurrentBalanceSum / model.MedalInputModelDb.FCFFactor);
			model.FreeCashFlow = model.AnnualTurnover == 0 ? 0 : (model.MedalInputModelDb.HmrcEbida - balance) / model.AnnualTurnover;
			model.TangibleEquity = model.AnnualTurnover == 0 ? 0 : model.MedalInputModelDb.TangibleEquity / model.AnnualTurnover;
			model.NumOfStores = model.MedalInputModelDb.NumOfStores;
			var mpHelper = new MarketPlacesHelper(Log);
			model.PositiveFeedbacks = mpHelper.GetPositiveFeedbacks(customerId);
			return model;
		}

		/// <summary>
		/// If HMRC turnover > 0.7*OnlineTurnover use HMRC turnover, otherwise, if yodlee turnover > 0.7*OnlineTurnover use yodlee turnover, otherwise use online turnover 
		///If it is negative – use 0 instead
		///The 0.7 should be configurable via UW – config name: OnlineMedalTurnoverCutoff
		/// </summary>
		private decimal GetOnlineAnnualTurnover(int customerId, MedalInputModelDb dbData) {
			var dbHelper = new DbHelper(Log);
			var mpHelper = new MarketPlacesHelper(Log);
			var yodlees = dbHelper.GetCustomerYodlees(customerId);
			var yodleeIncome = mpHelper.GetYodleeAnnualized(yodlees, Log);
			var onlineTurnover = mpHelper.GetOnlineTurnoverAnnualized(customerId);
			if (dbData.HmrcRevenues > onlineTurnover*dbData.OnlineMedalTurnoverCutoff) {
				return dbData.HmrcRevenues;
			}

			if (yodleeIncome > onlineTurnover * dbData.OnlineMedalTurnoverCutoff)
			{
				return yodleeIncome;
			}

			if (onlineTurnover < 0) {
				return 0;
			}

			return onlineTurnover;
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

			MedalOutputModel scoreMedal = CalcScoreMedalOffer(dict, MedalType.OnlineLimited);
			return scoreMedal;
		}

		protected override void CalcDelta(MedalInputModel model, Dictionary<Parameter, Weight> dict)
		{
			if (model.BusinessScore <= LowBusinessScore || model.ConsumerScore <= LowConsumerScore)
			{
				//Sum of all weights
				var sow = dict.Sum(x => x.Value.FinalWeight);
				//Sum of weights of of TangibleEquity, NetWorth, MaritalStatus, NumberOfStores, PositiveFeedbacks
				var sonf = dict[Parameter.TangibleEquity].FinalWeight +
					dict[Parameter.NetWorth].FinalWeight +
					dict[Parameter.MaritalStatus].FinalWeight +
					dict[Parameter.NumOfStores].FinalWeight +
					dict[Parameter.PositiveFeedbacks].FinalWeight;

				var sonfDesired = sonf - sow + 1;
				var ratio = sonfDesired / sonf;
				dict[Parameter.TangibleEquity].FinalWeight *= ratio;
				dict[Parameter.NetWorth].FinalWeight *= ratio;
				dict[Parameter.MaritalStatus].FinalWeight *= ratio;
				dict[Parameter.NumOfStores].FinalWeight *= ratio;
				dict[Parameter.PositiveFeedbacks].FinalWeight *= ratio;
			}
		}
		
		#region WeightConstants
		#region Base Weight
		public override decimal BusinessScoreBaseWeight { get { return 0.20M; } }
		public override decimal FreeCashFlowBaseWeight { get { return 0.13M; } }
		public override decimal AnnualTurnoverBaseWeight { get { return 0.10M; } }
		public override decimal TangibleEquityBaseWeight { get { return 0.08M; } }
		public override decimal BusinessSeniorityBaseWeight { get { return 0.07M; } }
		public override decimal ConsumerScoreBaseWeight { get { return 0.20M; } }
		public override decimal NetWorthBaseWeight { get { return 0.10M; } }
		public override decimal MaritalStatusBaseWeight { get { return 0.05M; } }
		

		public override decimal NumOfStoresBaseWeight { get { return 0.02M; } }
		public override decimal PositiveFeedbacksBaseWeight { get { return 0.05M; } }
		#endregion

		#region No HMRC Weight
		public override decimal FreeCashFlowNoHmrcWeight { get { return 0; } }

		public override decimal AnnualTurnoverNoHmrcWeightChange { get { return 0.05M; } }
		public override decimal BusinessScoreNoHmrcWeightChange { get { return 0.03M; } }
		public override decimal ConsumerScoreNoHmrcWeightChange { get { return 0.03M; } }
		public override decimal BusinessSeniorityNoHmrcWeightChange { get { return 0.04M; } }
		#endregion

		#region Low Score Weight
		public override decimal BusinessScoreLowScoreWeight { get { return 0.2750M; } }
		public override decimal ConsumerScoreLowScoreWeight { get { return 0.2750M; } }
		#endregion

		#region First Repayment Passed Weight
		public override decimal EzbobSeniorityFirstRepaymentWeight { get { return 0.02M; } }
		public override decimal NumOfOnTimeLoansFirstRepaymentWeight { get { return 0.0333M; } }
		public override decimal NumOfLateRepaymentsFirstRepaymentWeight { get { return 0.0267M; } }
		public override decimal NumOfEarlyRepaymentsFirstRepaymentWeight { get { return 0.02M; } }

		public override decimal ConsumerScoreFirstRepaymentWeightChange { get { return -0.04M; } }
		public override decimal BusinessScoreFirstRepaymentWeightChange { get { return -0.04M; } }
		public override decimal BusinessSeniorityFirstRepaymentWeightChange { get { return -0.02M; } }
		#endregion
		#endregion
	}
}
