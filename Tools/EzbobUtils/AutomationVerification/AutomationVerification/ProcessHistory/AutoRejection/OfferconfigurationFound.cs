namespace AutomationCalculator.ProcessHistory.AutoRejection {
	public class OfferConfigurationFound : ANumericTrace {
		public OfferConfigurationFound(DecisionStatus status) : base(status) {
		} // constructor

		protected override string ValueStr {
			get {
				int cfgCount = (int)Value;

				if (cfgCount < 1)
					return "no matched offer configurations";

				if (cfgCount > 1)
					return string.Format("too many ({0}) matched offer configurations", cfgCount);

				return "one matched offer configuration";
			} // get
		} // ValueStr
	} // class OfferConfigurationFound
} // namespace
