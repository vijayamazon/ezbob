namespace Ezbob.Backend.Strategies.MedalCalculations.Primary.Online {
	public class Limited : MedalBase {
		public override void SetInitialWeights() {
			Results.BusinessScoreWeight = 20;
			Results.FreeCashFlowWeight = 13;
			Results.AnnualTurnoverWeight = 10;
			Results.TangibleEquityWeight = 8;
			Results.BusinessSeniorityWeight = 7;
			Results.ConsumerScoreWeight = 20;
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
			Results.MedalType = MedalType.OnlineLimited;
		} // SetMedalType

		protected override decimal GetConsumerScoreWeightForLowScore() {
			return 27.5m;
		} // GetConsumerScoreWeightForLowScore

		protected override decimal GetCompanyScoreWeightForLowScore() {
			return 27.5m;
		} // GetCompanyScoreWeightForLowScore

		protected override void RedistributeFreeCashFlowWeight() {
			Results.FreeCashFlowWeight = 0;
			Results.AnnualTurnoverWeight += 5;
			Results.BusinessScoreWeight += 3;
			Results.ConsumerScoreWeight += 3;
			Results.BusinessSeniorityWeight += 2;
		} // RedistributeFreeCashFlowWeight

		protected override void RedistributeWeightsForPayingCustomer() {
			Results.BusinessScoreWeight -= 4;
			Results.BusinessSeniorityWeight -= 2;
			Results.ConsumerScoreWeight -= 4;
		} // RedistributeWeightsForPayingCustomer

		protected override decimal GetSumOfNonFixedWeights() {
			return
				Results.TangibleEquityWeight +
					Results.NetWorthWeight +
					Results.MaritalStatusWeight +
					Results.NumberOfStoresWeight +
					Results.PositiveFeedbacksWeight;
		} // GetSumOfNonFixedWeights

		protected override void AdjustWeightsWithRatio(decimal ratio) {
			Results.TangibleEquityWeight *= ratio;
			Results.NetWorthWeight *= ratio;
			Results.MaritalStatusWeight *= ratio;
			Results.NumberOfStoresWeight *= ratio;
			Results.PositiveFeedbacksWeight *= ratio;
		} // AdjustWeightsWithRatio
	} // Limited
} // namespace
