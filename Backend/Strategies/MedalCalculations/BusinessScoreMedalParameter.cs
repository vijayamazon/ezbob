namespace EzBob.Backend.Strategies.MedalCalculations
{
	public class BusinessScoreMedalParameter : MedalParameter
	{
		private readonly bool hasFreeCashFlowData;
		private readonly bool firstRepaymentDatePassed;
		public int BusinessScore { get; private set; }

		public BusinessScoreMedalParameter(int businessScore, bool hasFreeCashFlowData, bool firstRepaymentDatePassed)
		{
			Weight = 30;
			IsWeightFixed = true;
			MinGrade = 0;
			MaxGrade = 9;

			BusinessScore = businessScore;
			this.hasFreeCashFlowData = hasFreeCashFlowData;
			this.firstRepaymentDatePassed = firstRepaymentDatePassed;
		}

		public override void CalculateWeight()
		{
			if (BusinessScore <= 30)
			{
				Weight = 41.25m;
			}

			if (!hasFreeCashFlowData)
			{
				Weight += 5;
			}

			if (firstRepaymentDatePassed)
			{
				Weight -= 6.25m;
			}
		}

		public override void CalculateGrade()
		{
			if (BusinessScore < 11)
			{
				Grade = 0;
			}
			else if (BusinessScore < 21)
			{
				Grade = 1;
			}
			else if (BusinessScore < 31)
			{
				Grade = 2;
			}
			else if (BusinessScore < 41)
			{
				Grade = 3;
			}
			else if (BusinessScore < 51)
			{
				Grade = 4;
			}
			else if (BusinessScore < 61)
			{
				Grade = 5;
			}
			else if (BusinessScore < 71)
			{
				Grade = 6;
			}
			else if (BusinessScore < 81)
			{
				Grade = 7;
			}
			else if (BusinessScore < 91)
			{
				Grade = 8;
			}
			else
			{
				Grade = 9;
			}
		}
	}
}
