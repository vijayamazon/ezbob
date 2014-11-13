namespace EzBob.Backend.Strategies.Broker
{
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MedalCalculations;

	public class LimitedMedalCalculatorForInstantOffer : LimitedMedalCalculator1
	{
		private readonly int businessScore;
		private readonly decimal freeCashFlow;
		private readonly decimal annualTurnover;
		private readonly decimal tangibleEquity;
		private readonly DateTime? businessSeniority;
		private readonly int consumerScore;
		private readonly decimal netWorth;

		public LimitedMedalCalculatorForInstantOffer(int businessScore, decimal freeCashFlow, decimal annualTurnover, decimal tangibleEquity, DateTime? businessSeniority, int consumerScore, decimal netWorth, AConnection db, ASafeLog log)
			: base(db, log)
		{
			this.businessScore = businessScore;
			this.freeCashFlow = freeCashFlow;
			this.annualTurnover = annualTurnover;
			this.tangibleEquity = tangibleEquity;
			this.businessSeniority = businessSeniority;
			this.consumerScore = consumerScore;
			this.netWorth = netWorth;
		}

		protected override void GatherInputData(DateTime calculationTime)
		{
			// Filling fake data for instant offer
			Results.BusinessScore = businessScore;
			Results.FreeCashFlow = freeCashFlow;
			Results.AnnualTurnover = annualTurnover;
			Results.TangibleEquity = tangibleEquity;
			Results.BusinessSeniority = businessSeniority;
			Results.ConsumerScore = consumerScore;
			Results.NetWorth = netWorth;
			Results.MaritalStatus = MaritalStatus.Married; //todo?
			Results.EzbobSeniority = DateTime.Today;
			Results.NumOfEarlyRepayments = 0;
			Results.NumOfLateRepayments = 0;
			Results.NumOfLoans = 0;
		}
	}
}
