namespace EzBob.Backend.Strategies.ScoreCalculationVerification
{
	public class TangibleEquityMedalParameter : MedalParameter
	{
		public decimal TangibleEquity { get; private set; }

		public TangibleEquityMedalParameter(decimal tangibleEquity)
		{
			Weight = 8;
			IsWeightFixed = false;
			MinGrade = 0;
			MaxGrade = 4;

			TangibleEquity = tangibleEquity;
		}

		public override void CalculateGrade()
		{
			if (TangibleEquity < -0.05m)
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
