namespace EzBob.Backend.Strategies.LimitedMedalCalculation
{
	public class AnnualTurnoverMedalParameter : MedalParameter
	{
		private readonly bool hasFreeCashFlowData;
		public decimal AnnualTurnover { get; private set; }

		public AnnualTurnoverMedalParameter(decimal annualTurnover, bool hasFreeCashFlowData)
		{
			Weight = 10;
			IsWeightFixed = true;
			MinGrade = 0;
			MaxGrade = 6;

			AnnualTurnover = annualTurnover;
			this.hasFreeCashFlowData = hasFreeCashFlowData;
		}

		public override void CalculateWeight()
		{
			if (!hasFreeCashFlowData)
			{
				Weight += 7;
			}
		}

		public override void CalculateGrade()
		{
			if (AnnualTurnover < 30000)
			{
				Grade = 0;
			}
			else if (AnnualTurnover < 100000)
			{
				Grade = 1;
			}
			else if (AnnualTurnover < 200000)
			{
				Grade = 2;
			}
			else if (AnnualTurnover < 400000)
			{
				Grade = 3;
			}
			else if (AnnualTurnover < 800000)
			{
				Grade = 4;
			}
			else if (AnnualTurnover < 2000000)
			{
				Grade = 5;
			}
			else
			{
				Grade = 6;
			}
		}
	}
}
