namespace Ezbob.Backend.Strategies.MedalCalculations.Primary.Offline {
	public class NonLimited : MedalBase {
		public override void SetInitialWeights() {
			Results.BusinessScoreWeight = 21;
			Results.FreeCashFlowWeight = 15;
			Results.AnnualTurnoverWeight = 10;
			Results.TangibleEquityWeight = 0;
			Results.BusinessSeniorityWeight = 8;
			Results.ConsumerScoreWeight = 30;
			Results.NetWorthWeight = 10;
			Results.MaritalStatusWeight = 6;
			Results.NumberOfStoresWeight = 0;
			Results.PositiveFeedbacksWeight = 0;
			Results.EzbobSeniorityWeight = 0;
			Results.NumOfLoansWeight = 0;
			Results.NumOfLateRepaymentsWeight = 0;
			Results.NumOfEarlyRepaymentsWeight = 0;
		} // SetInitialWeights

		protected override void SetMedalType() {
			Results.MedalType = MedalType.NonLimited;
		} // SetMedalType

		protected override decimal GetConsumerScoreWeightForLowScore() {
			return 41.25m;
		} // GetConsumerScoreWeightForLowScore

		protected override decimal GetCompanyScoreWeightForLowScore() {
			return 28.875m;
		} // GetCompanyScoreWeightForLowScore

		protected override void RedistributeFreeCashFlowWeight() {
			Results.FreeCashFlowWeight = 0;
			Results.AnnualTurnoverWeight += 5;
			Results.BusinessScoreWeight += 4;
			Results.ConsumerScoreWeight += 4;
			Results.BusinessSeniorityWeight += 2;
		} // RedistributeFreeCashFlowWeight

		protected override void RedistributeWeightsForPayingCustomer() {
			Results.BusinessScoreWeight -= 4;
			Results.BusinessSeniorityWeight -= 2;
			Results.ConsumerScoreWeight -= 4;
		} // RedistributeWeightsForPayingCustomer

		protected override decimal GetSumOfNonFixedWeights() {
			return Results.NetWorthWeight + Results.MaritalStatusWeight;
		} // GetSumOfNonFixedWeights

		protected override void AdjustWeightsWithRatio(decimal ratio) {
			Results.NetWorthWeight *= ratio;
			Results.MaritalStatusWeight *= ratio;
		} // AdjustWeightsWithRatio
	} // class NonLimited
} // namespace
