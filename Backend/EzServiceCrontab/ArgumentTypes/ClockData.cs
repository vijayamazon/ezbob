namespace EzServiceCrontab.ArgumentTypes {
	using System;
	using System.Globalization;

	internal class ClockData : AType<DateTime> {
		public ClockData() : base("DateTime") {}

		public override object CreateInstance(string sValue) {
			DateTime nResult;

			if (DateTime.TryParse(sValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out nResult))
				return nResult;

			throw GetError(sValue);
		} // CreateInstance

	} // class ClockData
} // namespace
