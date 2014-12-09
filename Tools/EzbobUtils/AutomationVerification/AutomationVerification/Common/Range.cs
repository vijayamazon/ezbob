namespace AutomationCalculator.Common
{
	using System.Collections.Generic;
	using System.Linq;

	public class Range
	{
		public decimal? MinValue { get; set; }
		public decimal? MaxValue { get; set; }
		private readonly bool _includeUpper;
		private readonly bool _includeLower;

		public Range(bool includeUpper, bool includeLower) {
			_includeUpper = includeUpper;
			_includeLower = includeLower;
		}

		public Range(): this(false,true) { }

		public bool IsInRange(decimal value)
		{
			if (_includeLower && _includeUpper) {
				if (MinValue.HasValue && MaxValue.HasValue && value >= MinValue.Value && value <= MaxValue.Value) return true;
				if (MaxValue.HasValue && !MinValue.HasValue && value <= MaxValue.Value) return true;
				if (MinValue.HasValue && !MaxValue.HasValue && value >= MinValue.Value) return true;
			}

			if (!_includeLower && _includeUpper)
			{
				if (MinValue.HasValue && MaxValue.HasValue && value > MinValue.Value && value <= MaxValue.Value) return true;
				if (MaxValue.HasValue && !MinValue.HasValue && value <= MaxValue.Value) return true;
				if (MinValue.HasValue && !MaxValue.HasValue && value > MinValue.Value) return true;
			}

			if (_includeLower && !_includeUpper)
			{
				if (MinValue.HasValue && MaxValue.HasValue && value >= MinValue.Value && value < MaxValue.Value) return true;
				if (MaxValue.HasValue && !MinValue.HasValue && value < MaxValue.Value) return true;
				if (MinValue.HasValue && !MaxValue.HasValue && value >= MinValue.Value) return true;
			}

			if (!_includeLower && !_includeUpper)
			{
				if (MinValue.HasValue && MaxValue.HasValue && value > MinValue.Value && value < MaxValue.Value) return true;
				if (MaxValue.HasValue && !MinValue.HasValue && value < MaxValue.Value) return true;
				if (MinValue.HasValue && !MaxValue.HasValue && value > MinValue.Value) return true;
			}

			return false;
		}
	}

	public class RangeGrage : Range
	{
		public RangeGrage(bool includeUpper, bool includeLower) : base(includeUpper, includeLower) {}
		public RangeGrage() : this(false, true) {}

		public int Grade { get; set; }
	}

	public class RangeMedal : Range
	{
		public RangeMedal(bool includeUpper, bool includeLower) : base(includeUpper, includeLower) { }
		public RangeMedal() : this(true, false) {}

		public Medal Medal { get; set; }
	}

	public class RangeOfferPercent : Range
	{
		public decimal OfferPercent { get; set; }
	}

	public static class RangeExtension
	{
		public static int MinGrade(this IEnumerable<RangeGrage> rangeGrages)
		{
			return rangeGrages.Min(x => x.Grade);
		}

		public static int MaxGrade(this IEnumerable<RangeGrage> rangeGrages)
		{
			return rangeGrages.Max(x => x.Grade);
		}

		public static T GetRange<T>(this IEnumerable<T> ranges, decimal value) where T : Range
		{
			return ranges.FirstOrDefault(rangeGrage => rangeGrage.IsInRange(value));
		}

	}
}
