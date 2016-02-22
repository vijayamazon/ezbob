namespace Ezbob.Backend.Strategies.MedalCalculations.Primary.Online.NonLimited {
	public class NoBusinessScore : MedalBase {
		public override void SetInitialWeights() {
			Results.BusinessScoreWeight = 0;
			Results.FreeCashFlowWeight = 0;
			Results.AnnualTurnoverWeight = 11;
			Results.TangibleEquityWeight = 0;
			Results.BusinessSeniorityWeight = 15;
			Results.ConsumerScoreWeight = 40;
			Results.NetWorthWeight = 10;
			Results.MaritalStatusWeight = 5;
			Results.NumberOfStoresWeight = 4;
			Results.PositiveFeedbacksWeight = 15;
			Results.EzbobSeniorityWeight = 0;
			Results.NumOfLoansWeight = 0;
			Results.NumOfLateRepaymentsWeight = 0;
			Results.NumOfEarlyRepaymentsWeight = 0;
		} // SetInitialWeights

		protected override void SetMedalType() {
			Results.MedalType = MedalType.OnlineNonLimitedNoBusinessScore;
		} // SetMedalType

		protected override decimal GetConsumerScoreWeightForLowScore() {
			return 55;
		} // GetConsumerScoreWeightForLowScore

		protected override decimal GetCompanyScoreWeightForLowScore() {
			return 0;
		} // GetCompanyScoreWeightForLowScore

		protected override void RedistributeWeightsForPayingCustomer() {
			Results.NetWorthWeight -= 2;
			Results.BusinessSeniorityWeight -= 3;
			Results.ConsumerScoreWeight -= 5;
		} // RedistributeWeightsForPayingCustomer

		protected override decimal GetSumOfNonFixedWeights() {
			return Results.MaritalStatusWeight +
				Results.NumberOfStoresWeight +
				Results.PositiveFeedbacksWeight +
				Results.AnnualTurnoverWeight;
		} // GetSumOfNonFixedWeights

		protected override void AdjustWeightsWithRatio(decimal ratio) {
			Results.MaritalStatusWeight *= ratio;
			Results.NumberOfStoresWeight *= ratio;
			Results.PositiveFeedbacksWeight *= ratio;
			Results.AnnualTurnoverWeight *= ratio;
		} // AdjustWeightsWithRatio
	} // class NoBusinessScore
} // namespace
