namespace EzBob.Backend.Strategies.MedalCalculations
{
	using EZBob.DatabaseLib.Model.Database;

	public class MaritalStatusMedalParameter : MedalParameter
	{
		public MaritalStatus MaritalStatus { get; private set; }

		public MaritalStatusMedalParameter(MaritalStatus maritalStatus)
		{
			Weight = 5;
			IsWeightFixed = false;
			MinGrade = 2;
			MaxGrade = 4;

			MaritalStatus = maritalStatus;
		}

		public override void CalculateGrade()
		{
			if (MaritalStatus == MaritalStatus.Married || MaritalStatus == MaritalStatus.Widowed)
			{
				Grade = 4;
			}
			else if (MaritalStatus == MaritalStatus.Divorced || MaritalStatus == MaritalStatus.LivingTogether)
			{
				Grade = 3;
			}
			else // Single, Separated, Other
			{
				Grade = 2;
			}
		}
	}
}
