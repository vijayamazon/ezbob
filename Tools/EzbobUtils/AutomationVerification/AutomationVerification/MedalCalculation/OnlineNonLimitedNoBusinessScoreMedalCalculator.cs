namespace AutomationCalculator.MedalCalculation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Common;
	using Ezbob.Logger;

	public class OnlineNonLimitedNoBusinessScoreMedalCalculator : MedalCalculator
	{
		public OnlineNonLimitedNoBusinessScoreMedalCalculator(ASafeLog log) : base(log) { }

		public override MedalInputModel GetInputParameters(int customerId, DateTime? calculationDate = null)
		{
			var model = base.GetInputParameters(customerId);
			bool usingHmrc;
			model.AnnualTurnover = GetOnlineAnnualTurnover(customerId, model.MedalInputModelDb, out usingHmrc);
			model.FreeCashFlow = model.AnnualTurnover <= 0 || !usingHmrc ? 0 : model.MedalInputModelDb.HmrcFreeCashFlow;
			model.TangibleEquity = model.AnnualTurnover == 0 ? 0 : model.MedalInputModelDb.TangibleEquity / model.AnnualTurnover;
			model.NumOfStores = model.MedalInputModelDb.NumOfStores;
			var mpHelper = new MarketPlacesHelper(Log);
			model.PositiveFeedbacks = mpHelper.GetPositiveFeedbacks(customerId);
			return model;
		}
		
		public override MedalOutputModel CalculateMedal(MedalInputModel model)
		{
			//Log.Debug(model.ToString());

			var dict = new Dictionary<Parameter, Weight>
				{
					
					{Parameter.BusinessScore,            GetBusinessScoreWeight(model.BusinessScore, model.FirstRepaymentDatePassed, model.HasHmrc)},
					{Parameter.BusinessSeniority,        GetBusinessSeniorityWeight(model.BusinessSeniority, model.FirstRepaymentDatePassed, model.HasHmrc)},
					{Parameter.ConsumerScore,            GetConsumerScoreWeight(model.ConsumerScore, model.FirstRepaymentDatePassed, model.HasHmrc)},
					{Parameter.EzbobSeniority,           GetEzbobSeniorityWeight(model.EzbobSeniority, model.FirstRepaymentDatePassed)},
					{Parameter.MaritalStatus,            GetMaritalStatusWeight(model.MaritalStatus)},
					{Parameter.NumOfOnTimeLoans,         GetNumOfOnTimeLoansWeight(model.NumOfOnTimeLoans, model.FirstRepaymentDatePassed)},
					{Parameter.NumOfLatePayments,        GetNumOfLatePaymentsWeight(model.NumOfLatePayments, model.FirstRepaymentDatePassed)},
					{Parameter.NumOfEarlyPayments,       GetNumOfEarlyPaymentsWeight(model.NumOfEarlyPayments, model.FirstRepaymentDatePassed)},
					{Parameter.AnnualTurnover,           GetAnnualTurnoverWeight(model.AnnualTurnover, model.HasHmrc)},
					{Parameter.NetWorth,                 GetNetWorthWeight(model.NetWorth, model.FirstRepaymentDatePassed)},
					{Parameter.NumOfStores,              GetNumOfStoresWeight(model.NumOfStores)},
					{Parameter.PositiveFeedbacks,        GetPositiveFeedbacksWeight(model.PositiveFeedbacks)}
				};

			CalcDelta(model, dict);

			MedalOutputModel scoreMedal = CalcScoreMedalOffer(dict, model, MedalType.OnlineLimited);
			return scoreMedal;
		}

		protected override void CalcDelta(MedalInputModel model, Dictionary<Parameter, Weight> dict)
		{
			if (model.BusinessScore <= LowBusinessScore || model.ConsumerScore <= LowConsumerScore)
			{
				//Sum of all weights
				var sow = dict.Sum(x => x.Value.FinalWeight);
				//Sum of weights of of AnnualTurnover, MaritalStatus, NumberOfStores, PositiveFeedbacks
				var sonf = dict[Parameter.AnnualTurnover].FinalWeight +
					dict[Parameter.MaritalStatus].FinalWeight +
					dict[Parameter.NumOfStores].FinalWeight +
					dict[Parameter.PositiveFeedbacks].FinalWeight;

				var sonfDesired = sonf - sow + 1;
				var ratio = sonfDesired / sonf;
				dict[Parameter.AnnualTurnover].FinalWeight *= ratio;
				dict[Parameter.MaritalStatus].FinalWeight *= ratio;
				dict[Parameter.NumOfStores].FinalWeight *= ratio;
				dict[Parameter.PositiveFeedbacks].FinalWeight *= ratio;
			}
		}
		
		#region WeightConstants
		#region Base Weight
		public override decimal BusinessScoreBaseWeight { get { return 0.0M; } }
		public override decimal FreeCashFlowBaseWeight { get { return 0.25M; } }
		public override decimal AnnualTurnoverBaseWeight { get { return 0.16M; } }
		public override decimal TangibleEquityBaseWeight { get { return 0.0M; } }
		public override decimal BusinessSeniorityBaseWeight { get { return 0.03M; } }
		public override decimal ConsumerScoreBaseWeight { get { return 0.40M; } }
		public override decimal NetWorthBaseWeight { get { return 0.10M; } }
		public override decimal MaritalStatusBaseWeight { get { return 0.06M; } }
		
		public override decimal NumOfStoresBaseWeight { get { return 0.04M; } }
		public override decimal PositiveFeedbacksBaseWeight { get { return 0.15M; } }
		#endregion

		#region No HMRC Weight
		public override decimal FreeCashFlowNoHmrcWeight { get { return 0; } }

		public override decimal AnnualTurnoverNoHmrcWeightChange { get { return 0.0M; } }
		public override decimal BusinessScoreNoHmrcWeightChange { get { return 0.0M; } }
		public override decimal ConsumerScoreNoHmrcWeightChange { get { return 0.0M; } }
		public override decimal BusinessSeniorityNoHmrcWeightChange { get { return 0.0M; } }
		#endregion

		#region Low Score Weight
		public override decimal BusinessScoreLowScoreWeight { get { return 0; } }
		public override decimal ConsumerScoreLowScoreWeight { get { return 0.55M; } }
		#endregion

		#region First Repayment Passed Weight
		public override decimal ConsumerScoreFirstRepaymentWeightChange { get { return -0.05M; } }
		public override decimal BusinessScoreFirstRepaymentWeightChange { get { return -0.0M; } }
		public override decimal BusinessSeniorityFirstRepaymentWeightChange { get { return -0.03M; } }
		public override decimal NetWorthFirstRepaymentWeightChange { get { return -0.02M; } }
		#endregion
		#endregion
	}
}
