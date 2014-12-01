namespace AutomationCalculator.MedalCalculation
{
	using System.Collections.Generic;
	using System.Linq;
	using Common;
	using Ezbob.Database;
	using Ezbob.Logger;

	/// <summary>
	/// Medal calculated for customers that no other medal matched abd they have consumer score and (hmrc or bank market places)
	/// </summary>
	public class SoleTraderMedalCalculator : MedalCalculator
	{
		public SoleTraderMedalCalculator(AConnection db, ASafeLog log) : base(db, log) { }

		public override MedalOutputModel CalculateMedal(MedalInputModel model)
		{
			var dict = new Dictionary<Parameter, Weight>
				{
					
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

			MedalOutputModel scoreMedal = CalcScoreMedalOffer(dict, model, MedalType.SoleTrader);
			scoreMedal.FirstRepaymentDatePassed = model.FirstRepaymentDatePassed;
			scoreMedal.NumOfHmrcMps = model.HasHmrc ? 1 : 0;
			scoreMedal.CustomerId = model.CustomerId;
			scoreMedal.ValueAdded = model.ValueAdded;
			return scoreMedal;
		}

		protected override void CalcDelta(MedalInputModel model, Dictionary<Parameter, Weight> dict)
		{

			if (model.ConsumerScore <= LowConsumerScore)
			{
				//Sum of all weights
				var sow = dict.Sum(x => x.Value.FinalWeight);
				//Sum of weights of , NetWorth, MaritalStatus
				var sonf = dict[Parameter.NetWorth].FinalWeight + dict[Parameter.MaritalStatus].FinalWeight;

				var sonfDesired = sonf - sow + 1;
				var ratio = sonfDesired / sonf;
				dict[Parameter.NetWorth].FinalWeight *= ratio;
				dict[Parameter.MaritalStatus].FinalWeight *= ratio;
			}
		}

		#region WeightConstants
		#region Base Weight
		public override decimal BusinessScoreBaseWeight { get { return 0; } } //not in use
		public override decimal FreeCashFlowBaseWeight { get { return 0.25M; } }
		public override decimal AnnualTurnoverBaseWeight { get { return 0.16M; } }
		public override decimal BusinessSeniorityBaseWeight { get { return 0.03M; } }
		public override decimal ConsumerScoreBaseWeight { get { return 0.40M; } }
		public override decimal NetWorthBaseWeight { get { return 0.10M; } }
		public override decimal MaritalStatusBaseWeight { get { return 0.06M; } }
		#endregion

		#region No HMRC Weight
		public override decimal FreeCashFlowNoHmrcWeight { get { return 0; } }

		public override decimal AnnualTurnoverNoHmrcWeightChange { get { return 0.10M; } }
		public override decimal BusinessScoreNoHmrcWeightChange { get { return 0; } } //not in use
		public override decimal ConsumerScoreNoHmrcWeightChange { get { return 0.13M; } }
		public override decimal BusinessSeniorityNoHmrcWeightChange { get { return 0.02M; } }
		#endregion

		#region Low Score Weight
		public override decimal BusinessScoreLowScoreWeight { get { return 0; } } //not in use
		public override decimal ConsumerScoreLowScoreWeight { get { return 0.55M; } }
		#endregion

		#region First Repayment Passed Weight
		public override decimal ConsumerScoreFirstRepaymentWeightChange { get { return -0.09M; } }
		public override decimal BusinessScoreFirstRepaymentWeightChange { get { return 0; } } //not in use
		public override decimal BusinessSeniorityFirstRepaymentWeightChange { get { return -0.01M; } }
		#endregion
		#endregion
	}
}
