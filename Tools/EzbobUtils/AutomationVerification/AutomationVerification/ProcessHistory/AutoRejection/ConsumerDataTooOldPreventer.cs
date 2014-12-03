namespace AutomationCalculator.ProcessHistory.AutoRejection {
	using System;
	using System.Globalization;

	public class ConsumerDataTooOldPreventer : ATrace {
		public ConsumerDataTooOldPreventer(DecisionStatus nDecisionStatus) : base(nDecisionStatus) { }

		public DateTime? DataUpdateTime { get; private set; }
		public DateTime CurrentTime { get; private set; }

		public void Init(DateTime? oDataUpdateTime, DateTime oNow) {
			DataUpdateTime = oDataUpdateTime;
			CurrentTime = oNow;

			if (DataUpdateTime == null) {
				Comment = string.Format(
					"customer data has not been received before {0}",
					CurrentTime.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
				);
			}
			else {
				Comment = string.Format(
					"customer data has been received on {0}, current time is {1}",
					DataUpdateTime.Value.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
					CurrentTime.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
				);
			}
		} // Init
	} // class ConsumerDataTooOldPreventer
} // namespace
