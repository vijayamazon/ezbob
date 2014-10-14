namespace EzServiceCrontab.ArgumentTypes {
	using System;
	using System.Globalization;

	internal class ClockData : AType<DateTime> {
		public ClockData() : base("DateTime") {}

		#region method CreateInstance

		public override object CreateInstance(string sValue) {
			DateTime nResult;

			if (DateTime.TryParse(sValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out nResult))
				return nResult;

			throw GetError(sValue);
		} // CreateInstance

		#endregion method CreateInstance
	} // class ClockData
} // namespace
