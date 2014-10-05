namespace EzBob.Backend.Strategies.LimitedMedalCalculation
{
	public class OnTimeLoansMedalParameter : MedalParameter
	{
		private readonly bool firstRepaymentDatePassed;
		public int NumOfOnTimeLoans { get; private set; }

		public OnTimeLoansMedalParameter(int numOfOnTimeLoans, bool firstRepaymentDatePassed)
		{
			Weight = 0;
			IsWeightFixed = true;
			MinGrade = 1;
			MaxGrade = 4;

			NumOfOnTimeLoans = numOfOnTimeLoans;
			this.firstRepaymentDatePassed = firstRepaymentDatePassed;
		}

		public override void CalculateWeight()
		{
			if (firstRepaymentDatePassed)
			{
				Weight = 3.33m;
			}
		}

		public override void CalculateGrade()
		{
			if (NumOfOnTimeLoans < 2)
			{
				Grade = 1;
			}
			else if (NumOfOnTimeLoans < 4)
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
