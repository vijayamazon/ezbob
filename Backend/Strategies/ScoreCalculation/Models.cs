namespace Ezbob.Backend.Strategies.ScoreCalculation
{
	public class ScoreMedalOffer
	{
		public string AcParameters { get; set; }
		public string AcDescriptors { get; set; }
		public string ResultWeigts { get; set; }
		public string ResultMaxPoints { get; set; }

		public MedalMultiplier Medal { get; set; }
		public decimal ScorePoints { get; set; }
		public decimal ScoreResult { get; set; }
		public int MaxOffer { get; set; }
		public decimal MaxOfferPercent { get; set; }
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

	public class Range
	{
		public decimal? MinValue { get; set; }
		public decimal? MaxValue { get; set; }

		public bool IsInRange(decimal value)
		{
			if (MinValue.HasValue && MaxValue.HasValue && value >= MinValue.Value && value <= MaxValue.Value) return true;
			if (MaxValue.HasValue && !MinValue.HasValue && value <= MaxValue.Value) return true;
			if (MinValue.HasValue && !MaxValue.HasValue && value >= MinValue.Value) return true;
			return false;
		}
	}

	public class RangeGrage : Range
	{
		public int Grade { get; set; }
	}

	public class RangeMedal : Range
	{
		public MedalMultiplier Medal { get; set; }
	}

	public class RangeOfferPercent : Range
	{
		public decimal OfferPercent { get; set; }
	}
}
