namespace EzBob.Backend.Strategies.ScoreCalculationVerification
{
	public class FreeCashFlowMedalParameter : MedalParameter
	{
		private readonly bool hasHmrc;
		private readonly bool hasNonZeroTurnover;
		public decimal FreeCashFlow { get; private set; }

		public FreeCashFlowMedalParameter(decimal freeCashFlow, bool hasHmrc, bool hasNonZeroTurnover)
		{
			Weight = 19;
			IsWeightFixed = true;
			MinGrade = 0;
			MaxGrade = 6;

			FreeCashFlow = freeCashFlow;
			this.hasHmrc = hasHmrc;
			this.hasNonZeroTurnover = hasNonZeroTurnover;
		}

		public override void CalculateWeight()
		{
			if (!hasHmrc)
			{
				Weight = 0;
			}
		}

		public override void CalculateGrade()
		{
			if (FreeCashFlow < -0.1m || !hasHmrc || !hasNonZeroTurnover) // When turnover is zero we can't calc FCF, we want to keep the weight and have the min grade
			{
				Grade = 0;
			}
			else if (FreeCashFlow < 0)
			{
				Grade = 1;
			}
			else if (FreeCashFlow < 0.1m)
			{
				Grade = 2;
			}
			else if (FreeCashFlow < 0.2m)
			{
				Grade = 3;
			}
			else if (FreeCashFlow < 0.3m)
			{
				Grade = 4;
			}
			else if (FreeCashFlow < 0.4m)
			{
				Grade = 5;
			}
			else
			{
				Grade = 6;
			}
		}
	}
}
