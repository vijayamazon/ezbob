namespace EzBob.Backend.Strategies.LimitedMedalCalculation
{
	public class LateRepaymentsMedalParameter : MedalParameter
	{
		private readonly bool firstRepaymentDatePassed;
		public int NumOfLateRepayments { get; private set; }

		public LateRepaymentsMedalParameter(int numOfLateRepayments, bool firstRepaymentDatePassed)
		{
			Weight = 0;
			IsWeightFixed = true;
			MinGrade = 0;
			MaxGrade = 5;

			NumOfLateRepayments = numOfLateRepayments;
			this.firstRepaymentDatePassed = firstRepaymentDatePassed;
		}

		public override void CalculateWeight()
		{
			if (firstRepaymentDatePassed)
			{
				Weight = 2.67m;
			}
		}

		public override void CalculateGrade()
		{
			if (NumOfLateRepayments < 1)
			{
				Grade = 5;
			}
			else if (NumOfLateRepayments < 2)
			{
				Grade = 2;
			}
			else
			{
				Grade = 0;
			}
		}
	}
}
