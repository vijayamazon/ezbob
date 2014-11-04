namespace EzBob.Backend.Strategies.MedalCalculations
{
	public class EarlyRepaymentsMedalParameter : MedalParameter
	{
		private readonly bool firstRepaymentDatePassed;
		public int NumOfEarlyRepayments { get; private set; }

		public EarlyRepaymentsMedalParameter(int numOfEarlyRepayments, bool firstRepaymentDatePassed)
		{
			Weight = 0;
			IsWeightFixed = true;
			MinGrade = 2;
			MaxGrade = 5;

			NumOfEarlyRepayments = numOfEarlyRepayments;
			this.firstRepaymentDatePassed = firstRepaymentDatePassed;
		}

		public override void CalculateWeight()
		{
			if (firstRepaymentDatePassed)
			{
				Weight = 2;
			}
		}

		public override void CalculateGrade()
		{
			if (NumOfEarlyRepayments < 1)
			{
				Grade = 2;
			}
			else if (NumOfEarlyRepayments < 3)
			{
				Grade = 3;
			}
			else
			{
				Grade = 5;
			}
		}
	}
}
