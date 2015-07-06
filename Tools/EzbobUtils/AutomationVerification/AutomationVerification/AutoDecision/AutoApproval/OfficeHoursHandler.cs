namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;

	public class OfficeHoursHandler {
		public OfficeHoursHandler(
			DateTime now, // UTC time
			string officeStartTimeCfg,
			string officeEndTimeCfg,
			// ReSharper disable once UnusedParameter.Local
			string weekendCfg // Currently not used. Weekend is always Saturday & Sunday.
		) {
			Current = new CurrentValues(this);

			this.now = now;

			// Looks like .net knows when Summer time starts and ends so specifying GMT Standard Time
			// is enough, i.e. no need to calculate current offset from UTC--.net does this.
			//
			// Example 1:
			//     TimeZoneInfo londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
			//     DateTime now = new DateTime(2015, 12, 6, 13, 0, 0, DateTimeKind.Utc);
			//     DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(now, londonTimeZone);
			//     Console.WriteLine("local now: {0}", localNow.ToString("yyyy-MM-dd HH:mm:ss"));
			//  Prints: 2015-12-06 13:00:00
			//
			// Example 2:
			//     TimeZoneInfo londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
			//     DateTime now = new DateTime(2015, 7, 6, 13, 0, 0, DateTimeKind.Utc);
			//     DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(now, londonTimeZone);
			//     Console.WriteLine("local now: {0}", localNow.ToString("yyyy-MM-dd HH:mm:ss"));
			//  Prints: 2015-07-06 14:00:00

			TimeZoneInfo londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

			this.localNow = TimeZoneInfo.ConvertTimeFromUtc(now, londonTimeZone);
			this.startTime = ParseConfiguredTime(this.localNow, officeStartTimeCfg, 9, 0);
			this.endTime = ParseConfiguredTime(this.localNow, officeEndTimeCfg, 19, 0);

			// Index of array is a distance from now in days.
			// I.e. approvalCount[0] is approval count for today, openLoanAmount[1] is open loan amount for yesterday, etc.
			//                                                     0  1  2  3  4  5  6
			ApprovalCount  = new ArrayInterface<int>(    new [] {  0, 0, 0, 0, 0, 0, 0, });
			OpenLoanAmount = new ArrayInterface<decimal>(new [] { 0m, 0, 0, 0, 0, 0, 0, });
		} // constructor

		public ArrayInterface<int> ApprovalCount { get; private set; }
		public ArrayInterface<decimal> OpenLoanAmount { get; private set; }
		public CurrentValues Current { get; private set; }

		public bool IsWorkingTime {
			get { return !IsWeekend && !IsOffHours; }
		} // IsWorkingTime

		public class ArrayInterface<T> {
			public ArrayInterface(T[] data) {
				if (data == null)
					throw new ArgumentNullException("data", "Cannot initialize ArrayInterface from null reference.");

				this.data = data;
			} // constructor

			public int Length {
				get { return this.data.Length; }
			} // Length

			public T this[int idx] {
				get {
					return ((0 <= idx) && (idx < this.data.Length)) ? this.data[idx] : default (T);
				} // get
				set {
					if ((0 <= idx) && (idx < this.data.Length))
						this.data[idx] = value;
				} // set
			} // indexer

			private readonly T[] data;
		} // class ArrayInterface

		public class CurrentValues {
			public CurrentValues(OfficeHoursHandler ohh) {
				this.officeHoursHandler = ohh;
			} // constructor

			public int ApprovalCount {
				get {
					return (this.officeHoursHandler.localNow.DayOfWeek == DayOfWeek.Sunday)
						? this.officeHoursHandler.ApprovalCount[0] + this.officeHoursHandler.ApprovalCount[1]
						: this.officeHoursHandler.ApprovalCount[0];
				} // get
			} // ApprovalCount

			public decimal OpenLoanAmount {
				get {
					return (this.officeHoursHandler.localNow.DayOfWeek == DayOfWeek.Sunday)
						? this.officeHoursHandler.OpenLoanAmount[0] + this.officeHoursHandler.OpenLoanAmount[1]
						: this.officeHoursHandler.OpenLoanAmount[0];
				} // get
			} // ApprovalCount

			private readonly OfficeHoursHandler officeHoursHandler;
		} // class CurrentValues

		private bool IsWeekend {
			get { return this.localNow.DayOfWeek.In(DayOfWeek.Saturday, DayOfWeek.Sunday); }
		} // IsWeekend

		private bool IsOffHours {
			get { return (this.now < this.startTime) || (this.endTime < this.now); }
		} // IsOffHours

		private readonly DateTime now;
		private readonly DateTime localNow;
		private readonly DateTime startTime;
		private readonly DateTime endTime;

		/// <summary>
		/// Converts configured time string into actual UTC time.
		/// </summary>
		/// <param name="localNow">Current time in local London timezone.</param>
		/// <param name="configuredTime">Configured time. In London current local time timezone.
		/// String format: hour:minute.</param>
		/// <param name="defaultHour">Default hour if parsing of configured time failed.</param>
		/// <param name="defaultMinute">Default minute if parsing of configured time failed.</param>
		/// <returns>Parsed configured time or default value (if parsing fails).</returns>
		private static DateTime ParseConfiguredTime(
			DateTime localNow,
			string configuredTime,
			int defaultHour,
			int defaultMinute
		) {
			bool parsingFailed = string.IsNullOrWhiteSpace(configuredTime);

			string[] fields = null;

			int hour = defaultHour;
			int minute = defaultMinute;

			if (!parsingFailed) {
				fields = configuredTime.Trim().Split(':');

				if (fields.Length != 2)
					parsingFailed = true;
			} // if

			if (!parsingFailed)
				if (!int.TryParse(fields[0], out hour))
					parsingFailed = true;

			if (!parsingFailed)
				if ((hour < 0) || (hour > 23))
					parsingFailed = true;

			if (!parsingFailed)
				if (!int.TryParse(fields[1], out minute))
					parsingFailed = true;

			if (!parsingFailed)
				if ((minute < 0) || (minute > 59))
					parsingFailed = true;

			if (parsingFailed) {
				hour = defaultHour;
				minute = defaultMinute;
			} // if

			DateTime parsedValue = localNow.Date.AddHours(hour).AddMinutes(minute);

			return TimeZoneInfo.ConvertTimeToUtc(parsedValue);
		} // ParseConfiguredTime
	} // class OfficeHoursHandler

	internal static class DayOfWeekExt {
		public static bool In(this DayOfWeek dayOfWeek, params DayOfWeek[] lst) {
			foreach (DayOfWeek dw in lst)
				if (dw == dayOfWeek)
					return true;

			return false;
		} // In
	} // class DayOfWeekExt
} // namespace
