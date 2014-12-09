namespace AutomationCalculator.MedalCalculation
{
	using System.Collections.Generic;
	using System.Linq;
	using Common;
	using Ezbob.Database;
	using Ezbob.Logger;

	/// <summary>
	/// Medal calculated for customers that have non limited company with score and doesn't have online market places (ebay,amazon,paypal)
	/// </summary>
	public class NonLimitedMedalCalculator : MedalCalculator
	{
		public NonLimitedMedalCalculator(AConnection db, ASafeLog log) : base(db, log) { }

		public override MedalOutputModel CalculateMedal(MedalInputModel model)
		{
			var dict = new Dictionary<Parameter, Weight>
				{

					{Parameter.BusinessScore,      GetBusinessScoreWeight(model.BusinessScore, model.FirstRepaymentDatePassed, model.UseHmrc)},
					{Parameter.BusinessSeniority,  GetBusinessSeniorityWeight(model.BusinessSeniority, model.FirstRepaymentDatePassed, model.UseHmrc)},
					{Parameter.ConsumerScore,      GetConsumerScoreWeight(model.ConsumerScore, model.FirstRepaymentDatePassed, model.UseHmrc)},
					{Parameter.EzbobSeniority,     GetEzbobSeniorityWeight(model.EzbobSeniority, model.FirstRepaymentDatePassed)},
					{Parameter.MaritalStatus,      GetMaritalStatusWeight(model.MaritalStatus)},
					{Parameter.NumOfOnTimeLoans,   GetNumOfOnTimeLoansWeight(model.NumOfOnTimeLoans, model.FirstRepaymentDatePassed)},
					{Parameter.NumOfLatePayments,  GetNumOfLatePaymentsWeight(model.NumOfLatePayments, model.FirstRepaymentDatePassed)},
					{Parameter.NumOfEarlyPayments, GetNumOfEarlyPaymentsWeight(model.NumOfEarlyPayments, model.FirstRepaymentDatePassed)},
					{Parameter.AnnualTurnover,     GetAnnualTurnoverWeight(model.AnnualTurnover, model.UseHmrc)},
					{Parameter.FreeCashFlow,       GetFreeCashFlowWeight(model.FreeCashFlow, model.UseHmrc, model.AnnualTurnover)},
					{Parameter.NetWorth,           GetNetWorthWeight(model.NetWorth, model.FirstRepaymentDatePassed)},
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

		public override decimal BusinessScoreBaseWeight { get { return 0.21M; } }
		public override decimal FreeCashFlowBaseWeight { get { return 0.15M; } }
		public override decimal AnnualTurnoverBaseWeight { get { return 0.10M; } }
		public override decimal BusinessSeniorityBaseWeight { get { return 0.08M; } }
		public override decimal ConsumerScoreBaseWeight { get { return 0.30M; } }
		public override decimal NetWorthBaseWeight { get { return 0.10M; } }
		public override decimal MaritalStatusBaseWeight { get { return 0.06M; } }

		public override decimal FreeCashFlowNoHmrcWeight { get { return 0; } }

		public override decimal AnnualTurnoverNoHmrcWeightChange { get { return 0.05M; } }
		public override decimal BusinessScoreNoHmrcWeightChange { get { return 0.04M; } }
		public override decimal ConsumerScoreNoHmrcWeightChange { get { return 0.04M; } }
		public override decimal BusinessSeniorityNoHmrcWeightChange { get { return 0.02M; } }

		public override decimal BusinessScoreLowScoreWeight { get { return 0.28875M; } }
		public override decimal ConsumerScoreLowScoreWeight { get { return 0.4125M; } }

		public override decimal ConsumerScoreFirstRepaymentWeightChange { get { return -0.04M; } }
		public override decimal BusinessScoreFirstRepaymentWeightChange { get { return -0.04M; } }
		public override decimal BusinessSeniorityFirstRepaymentWeightChange { get { return -0.02M; } }

	}
}

