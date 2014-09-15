namespace EzBob.Backend.Strategies.ScoreCalculationVerification
{
	public class NetWorthMedalParameter : MedalParameter
	{
		public decimal NetWorth { get; private set; }

		public NetWorthMedalParameter(decimal netWorth)
		{
			Weight = 10;
			IsWeightFixed = false;
			MinGrade = 0;
			MaxGrade = 3;

			NetWorth = netWorth;
		}

		public override void CalculateGrade()
		{
			if (NetWorth < 0.15m)
			{
				Grade = 0;
			}
			else if (NetWorth < 0.5m)
			{
				Grade = 1;
			}
			else if (NetWorth < 1)
			{
				Grade = 2;
			}
			else
			{
				Grade = 3;
			}
		}
	}
}
