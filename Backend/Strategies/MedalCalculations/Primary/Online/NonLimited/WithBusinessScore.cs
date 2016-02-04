namespace Ezbob.Backend.Strategies.MedalCalculations.Primary.Online.NonLimited {
	public class WithBusinessScore : MedalBase {
		public override void SetInitialWeights() {
			Results.BusinessScoreWeight = 25;
			Results.FreeCashFlowWeight = 0;
			Results.AnnualTurnoverWeight = 13;
			Results.TangibleEquityWeight = 0;
			Results.BusinessSeniorityWeight = 5;
			Results.ConsumerScoreWeight = 35;
			Results.NetWorthWeight = 10;
			Results.MaritalStatusWeight = 5;
			Results.NumberOfStoresWeight = 2;
			Results.PositiveFeedbacksWeight = 5;
			Results.EzbobSeniorityWeight = 0;
			Results.NumOfLoansWeight = 0;
			Results.NumOfLateRepaymentsWeight = 0;
			Results.NumOfEarlyRepaymentsWeight = 0;
		} // SetInitialWeights

		protected override void SetMedalType() {
			Results.MedalType = MedalType.OnlineNonLimitedWithBusinessScore;
		} // SetMedalType

		protected override decimal GetConsumerScoreWeightForLowScore() {
			return 47;
		} // GetConsumerScoreWeightForLowScore

		protected override decimal GetCompanyScoreWeightForLowScore() {
			return 33;
		} // GetCompanyScoreWeightForLowScore

		protected override void RedistributeWeightsForPayingCustomer() {
			Results.BusinessScoreWeight -= 4;
			Results.BusinessSeniorityWeight -= 2;
			Results.ConsumerScoreWeight -= 4;
		} // RedistributeWeightsForPayingCustomer

		protected override decimal GetSumOfNonFixedWeights() {
			return Results.NetWorthWeight +
				Results.MaritalStatusWeight +
				Results.NumberOfStoresWeight +
				Results.PositiveFeedbacksWeight +
				Results.AnnualTurnoverWeight;
		} // GetSumOfNonFixedWeights

		protected override void AdjustWeightsWithRatio(decimal ratio) {
			Results.NetWorthWeight *= ratio;
			Results.MaritalStatusWeight *= ratio;
			Results.NumberOfStoresWeight *= ratio;
			Results.PositiveFeedbacksWeight *= ratio;
			Results.AnnualTurnoverWeight *= ratio;
		} // AdjustWeightsWithRatio
	} // class WithBusinessScore
} // namespace
