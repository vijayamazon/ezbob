namespace EzServiceCrontab.ArgumentTypes {
	using System;
	using System.Globalization;

	internal class ClockDataish : AType<DateTime?> {
		public ClockDataish() : base("DateTime") {}

		#region method CreateInstance

		public override object CreateInstance(string sValue) {
			DateTime nResult;

			if (DateTime.TryParse(sValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out nResult))
				return nResult;

			return Activator.CreateInstance(UnderlyingType);
		} // CreateInstance

		#endregion method CreateInstance
	} // class ClockDataish
} // namespace
