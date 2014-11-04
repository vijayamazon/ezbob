namespace EzBob.Backend.Strategies.MedalCalculations
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
				// We know that we sometimes miss mortgages the customer has, so instead of grade=3 we give 1
				Grade = 1;
			}
		}
	}
}
