namespace EzBob.Backend.Strategies.ScoreCalculationVerification
{
	public class ConsumerScoreMedalParameter : MedalParameter
	{
		private readonly bool hasHmrc;
		private readonly bool firstRepaymentDatePassed;
		public int ConsumerScore { get; private set; }

		public ConsumerScoreMedalParameter(int consumerScore, bool hasHmrc, bool firstRepaymentDatePassed)
		{
			Weight = 10;
			IsWeightFixed = true;
			MinGrade = 0;
			MaxGrade = 8;

			ConsumerScore = consumerScore;
			this.hasHmrc = hasHmrc;
			this.firstRepaymentDatePassed = firstRepaymentDatePassed;
		}

		public override void CalculateWeight()
		{
			if (ConsumerScore <= 800)
			{
				Weight = 13.75m;
			}

			if (!hasHmrc)
			{
				Weight += 3;
			}

			if (firstRepaymentDatePassed)
			{
				Weight -= 2.1m;
			}
		}

		public override void CalculateGrade()
		{
			if (ConsumerScore < 481)
			{
				Grade = 0;
			}
			else if (ConsumerScore < 561)
			{
				Grade = 1;
			}
			else if (ConsumerScore < 641)
			{
				Grade = 2;
			}
			else if (ConsumerScore < 721)
			{
				Grade = 3;
			}
			else if (ConsumerScore < 801)
			{
				Grade = 4;
			}
			else if (ConsumerScore < 881)
			{
				Grade = 5;
			}
			else if (ConsumerScore < 961)
			{
				Grade = 6;
			}
			else if (ConsumerScore < 1041)
			{
				Grade = 7;
			}
			else
			{
				Grade = 8;
			}
		}
	}
}
