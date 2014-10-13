namespace EzBob.Backend.Strategies.LimitedMedalCalculation
{
	using System;

	public class BusinessSeniorityMedalParameter : MedalParameter
	{
		private readonly bool hasHmrc;
		private readonly bool firstRepaymentDatePassed;
		private readonly int businessSeniorityYears;
		public DateTime? IncorporationDate { get; private set; }

		public BusinessSeniorityMedalParameter(DateTime? businessSeniority, bool hasHmrc, bool firstRepaymentDatePassed, DateTime calculationTime)
		{
			Weight = 8;
			IsWeightFixed = true;
			MinGrade = 0;
			MaxGrade = 4;

			IncorporationDate = businessSeniority;
			businessSeniorityYears = CalculateBusinessSeniorityInFullYears(businessSeniority, calculationTime);
			this.hasHmrc = hasHmrc;
			this.firstRepaymentDatePassed = firstRepaymentDatePassed;
		}

		private int CalculateBusinessSeniorityInFullYears(DateTime? businessSeniority, DateTime calculationTime)
		{
			int numOfYears = 0;
			if (businessSeniority.HasValue)
			{
				numOfYears = calculationTime.Year - businessSeniority.Value.Year;
				if (calculationTime.Month < businessSeniority.Value.Month ||
					(calculationTime.Month == businessSeniority.Value.Month && calculationTime.Day < businessSeniority.Value.Day))
				{
					numOfYears--;
				}
				if (businessSeniorityYears < 0)
				{
					numOfYears = 0;
				}
			}

			return numOfYears;
		}

		public override void CalculateWeight()
		{
			if (!hasHmrc)
			{
				Weight += 4;
			}

			if (firstRepaymentDatePassed)
			{
				Weight -= 1.67m;
			}
		}

		public override void CalculateGrade()
		{
			if (businessSeniorityYears < 1)
			{
				Grade = 0;
			}
			else if (businessSeniorityYears < 3)
			{
				Grade = 1;
			}
			else if (businessSeniorityYears < 5)
			{
				Grade = 2;
			}
			else if (businessSeniorityYears < 10)
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
