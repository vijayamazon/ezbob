namespace EzBob.Backend.Strategies.MedalCalculations
{
	using System;
	using Ezbob.Utils;

	public class EzbobSeniorityMedalParameter : MedalParameter
	{
		private readonly bool firstRepaymentDatePassed;
		public DateTime? EzbobSeniority { get; private set; }
		public int EzbobSeniorityMonths { get; private set; }

		public EzbobSeniorityMedalParameter(DateTime? ezbobSeniority, bool firstRepaymentDatePassed)
		{
			Weight = 0;
			IsWeightFixed = true;
			MinGrade = 0;
			MaxGrade = 4;

			if (ezbobSeniority.HasValue)
			{
				EzbobSeniority = ezbobSeniority;
				int ezbobSeniorityMonthsOnly, ezbobSeniorityYearsOnly;
				MiscUtils.GetFullYearsAndMonths(ezbobSeniority.Value, out ezbobSeniorityYearsOnly, out ezbobSeniorityMonthsOnly);
				EzbobSeniorityMonths = ezbobSeniorityMonthsOnly + 12 * ezbobSeniorityYearsOnly;
			}
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
			if (EzbobSeniorityMonths < 1)
			{
				Grade = 0;
			}
			else if (EzbobSeniorityMonths < 6)
			{
				Grade = 2;
			}
			else if (EzbobSeniorityMonths < 18)
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
