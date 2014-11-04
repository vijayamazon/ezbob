namespace EzBob.Backend.Strategies.MedalCalculations
{
	public abstract class MedalParameter
	{
		public int Grade { get; set; }
		public decimal Weight { get; set; }
		public decimal Score { get; set; }
		public bool IsWeightFixed { get; set; } // Parameters with "fixed" weight don't participate in some of the weight distribution

		public int MinGrade { get; set; }
		public int MaxGrade { get; set; }

		public virtual void CalculateWeight() { }
		public abstract void CalculateGrade();
		public void CalculateScore()
		{
			Score = Weight * Grade;
		}
	}
}
