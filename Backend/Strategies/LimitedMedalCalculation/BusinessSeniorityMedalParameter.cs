namespace EzBob.Backend.Strategies.LimitedMedalCalculation
{
	using System;

	public class BusinessSeniorityMedalParameter : MedalParameter
	{
		private readonly bool hasHmrc;
		private readonly bool firstRepaymentDatePassed;
		private readonly int businessSeniorityMonths;
		public DateTime? IncorporationDate { get; private set; }

		public BusinessSeniorityMedalParameter(DateTime? businessSeniority, bool hasHmrc, bool firstRepaymentDatePassed)
		{
			Weight = 8;
			IsWeightFixed = true;
			MinGrade = 0;
			MaxGrade = 4;

			IncorporationDate = businessSeniority;
			businessSeniorityMonths = CalculateBusinessSeniorityInFullYears(businessSeniority);
			this.hasHmrc = hasHmrc;
			this.firstRepaymentDatePassed = firstRepaymentDatePassed;
		}

		private int CalculateBusinessSeniorityInFullYears(DateTime? businessSeniority)
		{
			int numOfMonths = 0;
			if (businessSeniority.HasValue)
			{
				DateTime now = DateTime.UtcNow;
				numOfMonths = now.Year - businessSeniority.Value.Year;
				if (now.Month < businessSeniority.Value.Month ||
					(now.Month == businessSeniority.Value.Month && now.Day < businessSeniority.Value.Day))
				{
					numOfMonths--;
				}
				if (businessSeniorityMonths < 0)
				{
					numOfMonths = 0;
				}
			}

			return numOfMonths;
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
			if (businessSeniorityMonths < 1)
			{
				Grade = 0;
			}
			else if (businessSeniorityMonths < 3)
			{
				Grade = 1;
			}
			else if (businessSeniorityMonths < 5)
			{
				Grade = 2;
			}
			else if (businessSeniorityMonths < 10)
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
