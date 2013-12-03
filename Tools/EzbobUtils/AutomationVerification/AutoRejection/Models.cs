namespace AutomationCalculator
{
	public class ScoreMedal
	{
		public Medal Medal { get; set; }
		public decimal Score { get; set; }
	}

	public class Weight
	{
		public decimal FinalWeightFixedWeightParameter { get; set; }
		public decimal StandardWeightFixedWeightParameter { get; set; }
		public decimal StandardWeightAdjustableWeightParameter { get; set; }
		public decimal DeltaForAdjustableWeightParameter { get; set; }
		public decimal FinalWeight { get; set; }
		public decimal MinimumScore { get; set; }
		public decimal MaximumScore { get; set; }
		public int MinimumGrade { get; set; }
		public int MaximumGrade { get; set; }

		public int Grade { get; set; }
		public decimal Score { get; set; }
	}

	public class RangeGrage
	{
		public int? MinValue { get; set; }
		public int? MaxValue { get; set; }
		public int Grade { get; set; }

		public bool IsInRange(decimal value)
		{
			if (MinValue.HasValue && MaxValue.HasValue && value >= MinValue.Value && value <= MaxValue.Value) return true;
			if (MaxValue.HasValue && !MinValue.HasValue && value <= MaxValue.Value) return true;
			if (MinValue.HasValue && !MaxValue.HasValue && value >= MinValue.Value) return true;
			return false;
		}
	}
}
