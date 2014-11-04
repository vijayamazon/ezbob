namespace EzBob.Backend.Strategies.MedalCalculations
{
	public class TangibleEquityMedalParameter : MedalParameter
	{
		private readonly bool hasNonZeroTurnover;
		public decimal TangibleEquity { get; private set; }

		public TangibleEquityMedalParameter(decimal tangibleEquity, bool hasNonZeroTurnover)
		{
			Weight = 8;
			IsWeightFixed = false;
			MinGrade = 0;
			MaxGrade = 4;

			TangibleEquity = tangibleEquity;
			this.hasNonZeroTurnover = hasNonZeroTurnover;
		}

		public override void CalculateGrade()
		{
			if (TangibleEquity < -0.05m || !hasNonZeroTurnover) // When turnover is zero we can't calc tangible equity, we want to keep the weight and have the min grade
			{
				Grade = 0;
			}
			else if (TangibleEquity < 0)
			{
				Grade = 1;
			}
			else if (TangibleEquity < 0.1m)
			{
				Grade = 2;
			}
			else if (TangibleEquity < 0.3m)
			{
				Grade = 3;
			}
			else
			{
				Grade = 4;
			}
		}
	}
}
