namespace Ezbob.Backend.Strategies.MedalCalculations.Primary.Offline {
	public class Limited : MedalBase {
		public override void SetInitialWeights() {
			Results.BusinessScoreWeight = 30;
			Results.FreeCashFlowWeight = 19;
			Results.AnnualTurnoverWeight = 10;
			Results.TangibleEquityWeight = 8;
			Results.BusinessSeniorityWeight = 8;
			Results.ConsumerScoreWeight = 10;
			Results.NetWorthWeight = 10;
			Results.MaritalStatusWeight = 5;
			Results.NumberOfStoresWeight = 0;
			Results.PositiveFeedbacksWeight = 0;
			Results.EzbobSeniorityWeight = 0;
			Results.NumOfLoansWeight = 0;
			Results.NumOfLateRepaymentsWeight = 0;
			Results.NumOfEarlyRepaymentsWeight = 0;
		} // SetInitialWeights

		protected override void SetMedalType() {
			Results.MedalType = MedalType.Limited;
		} // SetMedalType

		protected override decimal GetConsumerScoreWeightForLowScore() {
			return 13.75m;
		} // GetConsumerScoreWeightForLowScore

		protected override decimal GetCompanyScoreWeightForLowScore() {
			return 41.25m;
		} // GetCompanyScoreWeightForLowScore

		protected override void RedistributeFreeCashFlowWeight() {
			Results.FreeCashFlowWeight = 0;
			Results.AnnualTurnoverWeight += 7;
			Results.BusinessScoreWeight += 5;
			Results.ConsumerScoreWeight += 3;
			Results.BusinessSeniorityWeight += 4;
		} // RedistributeFreeCashFlowWeight

		protected override void RedistributeWeightsForPayingCustomer() {
			Results.BusinessScoreWeight -= 6.25m;
			Results.BusinessSeniorityWeight -= 1.67m;
			Results.ConsumerScoreWeight -= 2.08m;
		} // RedistributeWeightsForPayingCustomer

		protected override decimal GetSumOfNonFixedWeights() {
			return Results.TangibleEquityWeight + Results.NetWorthWeight + Results.MaritalStatusWeight;
		} // GetSumOfNonFixedWeights

		protected override void AdjustWeightsWithRatio(decimal ratio) {
			Results.TangibleEquityWeight *= ratio;
			Results.NetWorthWeight *= ratio;
			Results.MaritalStatusWeight *= ratio;
		} // AdjustWeightsWithRatio
	} // class Limited
} // namespace
