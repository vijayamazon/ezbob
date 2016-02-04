namespace Ezbob.Backend.Strategies.MedalCalculations.Primary.Offline {
	public class SoleTrader : MedalBase {
		public override void SetInitialWeights() {
			Results.BusinessScoreWeight = 0;
			Results.FreeCashFlowWeight = 25;
			Results.AnnualTurnoverWeight = 16;
			Results.TangibleEquityWeight = 0;
			Results.BusinessSeniorityWeight = 3;
			Results.ConsumerScoreWeight = 40;
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
			Results.MedalType = MedalType.SoleTrader;
		} // SetMedalType

		protected override decimal GetConsumerScoreWeightForLowScore() {
			return 55;
		} // GetConsumerScoreWeightForLowScore

		protected override decimal GetCompanyScoreWeightForLowScore() {
			return 0;
		} // GetCompanyScoreWeightForLowScore

		protected override void RedistributeFreeCashFlowWeight() {
			Results.FreeCashFlowWeight = 0;
			Results.AnnualTurnoverWeight += 10;
			Results.ConsumerScoreWeight += 13;
			Results.BusinessSeniorityWeight += 2;
		} // RedistributeFreeCashFlowWeight

		protected override void RedistributeWeightsForPayingCustomer() {
			Results.BusinessSeniorityWeight -= 1;
			Results.ConsumerScoreWeight -= 9;
		} // RedistributeWeightsForPayingCustomer

		protected override decimal GetSumOfNonFixedWeights() {
			return Results.NetWorthWeight + Results.MaritalStatusWeight;
		} // GetSumOfNonFixedWeights

		protected override void AdjustWeightsWithRatio(decimal ratio) {
			Results.NetWorthWeight *= ratio;
			Results.MaritalStatusWeight *= ratio;
		} // AdjustWeightsWithRatio
	} // class SoleTrader
} // namespace
