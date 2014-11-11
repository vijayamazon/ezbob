namespace AutomationCalculator.MedalCalculation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Common;
	using Ezbob.Logger;

	public class NonLimitedMedalCalculator : MedalCalculator
	{
		public NonLimitedMedalCalculator(ASafeLog log) : base(log) { }

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
					{Parameter.FreeCashFlow,             GetFreeCashFlowWeight(model.FreeCashFlow, model.HasHmrc, model.AnnualTurnover)},
					{Parameter.NetWorth,                 GetNetWorthWeight(model.NetWorth, model.FirstRepaymentDatePassed)},
				};

			CalcDelta(model, dict);

			MedalOutputModel scoreMedal = CalcScoreMedalOffer(dict, model, MedalType.NonLimited);
			return scoreMedal;
		}
		
		protected override void CalcDelta(MedalInputModel model, Dictionary<Parameter, Weight> dict)
		{

			if (model.BusinessScore <= LowBusinessScore || model.ConsumerScore <= LowConsumerScore)
			{
				//Sum of all weights
				var sow = dict.Sum(x => x.Value.FinalWeight);
				//Sum of weights of of TangibleEquity, NetWorth, MaritalStatus, NumberOfStores, PositiveFeedbacks
				var sonf = dict[Parameter.NetWorth].FinalWeight + dict[Parameter.MaritalStatus].FinalWeight;

				var sonfDesired = sonf - sow + 1;
				var ratio = sonfDesired / sonf;
				dict[Parameter.NetWorth].FinalWeight *= ratio;
				dict[Parameter.MaritalStatus].FinalWeight *= ratio;
			}
		}

		#region WeightConstants
		#region Base Weight
		public override decimal BusinessScoreBaseWeight { get { return 0.21M; } }
		public override decimal FreeCashFlowBaseWeight { get { return 0.15M; } }
		public override decimal AnnualTurnoverBaseWeight { get { return 0.10M; } }
		public override decimal BusinessSeniorityBaseWeight { get { return 0.08M; } }
		public override decimal ConsumerScoreBaseWeight { get { return 0.30M; } }
		public override decimal NetWorthBaseWeight { get { return 0.10M; } }
		public override decimal MaritalStatusBaseWeight { get { return 0.06M; } }
		#endregion

		#region No HMRC Weight
		public override decimal FreeCashFlowNoHmrcWeight { get { return 0; } }

		public override decimal AnnualTurnoverNoHmrcWeightChange { get { return 0.05M; } }
		public override decimal BusinessScoreNoHmrcWeightChange { get { return 0.04M; } }
		public override decimal ConsumerScoreNoHmrcWeightChange { get { return 0.04M; } }
		public override decimal BusinessSeniorityNoHmrcWeightChange { get { return 0.02M; } }
		#endregion

		#region Low Score Weight
		public override decimal BusinessScoreLowScoreWeight { get { return 0.28875M; } }
		public override decimal ConsumerScoreLowScoreWeight { get { return 0.4125M; } }
		#endregion

		#region First Repayment Passed Weight
		public override decimal ConsumerScoreFirstRepaymentWeightChange { get { return -0.04M; } }
		public override decimal BusinessScoreFirstRepaymentWeightChange { get { return -0.04M; } }
		public override decimal BusinessSeniorityFirstRepaymentWeightChange { get { return -0.02M; } }
		#endregion
		#endregion
	}
}


