namespace Ezbob.Backend.Strategies.Tasks {
	public class TotalMaamMedalAndPricing : WeeklyMaamMedalAndPricing {
		public TotalMaamMedalAndPricing() : base(true) {
		} // constructor

		public override string Name {
			get { return "TotalMaamMedalAndPricing"; }
		} // Name

		protected override string Condition {
			get { return string.Empty; }
		} // Condition
	} // class WeeklyMaamMedalAndPricing
} // namespace

