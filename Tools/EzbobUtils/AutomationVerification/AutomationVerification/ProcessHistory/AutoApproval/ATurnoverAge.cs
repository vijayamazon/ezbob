namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using System;
	using System.Globalization;

	public abstract class ATurnoverAge : ATrace {

		public virtual DateTime? UpdateTime { get; private set; }
		public virtual DateTime Now { get; private set; }

		public virtual void Init(DateTime? oUpdateTime, DateTime oNow) {
			UpdateTime = oUpdateTime;
			Now = oNow;

			if (UpdateTime == null) {
				Comment = string.Format(
					"{0} marketplace has not been updated ever, now is {1}",
					TurnoverName,
					oNow.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
				);
			}
			else {
				Comment = string.Format(
					"{0} marketplace has been updated on {2}, now is {1}",
					TurnoverName,
					oNow.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
					UpdateTime.Value.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
				);
			}
		} // Init

		protected abstract string TurnoverName { get; }

		protected ATurnoverAge(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

	} // class ATurnoverAge
} // namespace
