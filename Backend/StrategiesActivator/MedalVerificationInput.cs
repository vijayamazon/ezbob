namespace StrategiesActivator {
	using System;
	using System.Globalization;
	using Ezbob.Logger;

	internal class MedalVerificationInput : VerificationInput {
		public MedalVerificationInput(string sName, string[] args, ASafeLog oLog) : base(sName, args, oLog) {
		} // constructor

		public virtual bool IncludeTest { get; private set; }
		public virtual DateTime? CalculationTime { get; private set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"{0}, test: {1}, calculation time: {2}",
				base.ToString(),
				IncludeTest,
				CalculationTime == null
					? "today"
					: CalculationTime.Value.ToString("MMMM d yyyy", CultureInfo.InvariantCulture)
			);
		} // ToString

		protected override void ProcessArgs() {
			IncludeTest = false;
			CalculationTime = null;

			base.ProcessArgs();

			if (!IsGood)
				return;

			if (Args.Count == 0)
				return;

			bool b;

			IsGood = bool.TryParse(Args.Dequeue(), out b);

			if (IsGood)
				IncludeTest = b;
			else
				return;

			if (Args.Count == 0)
				return;

			DateTime d;

			IsGood = DateTime.TryParseExact(
				Args.Dequeue(),
				"yyyy-MM-dd",
				CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
				out d
			);

			if (IsGood)
				CalculationTime = d;
		} // ProcessArgs

		protected override void LogUsage() {
			Log.Msg(
				"Usage: {0} [ <count>  [ <last checked id> [ <include test customers> [ <calculation date> ] ] ] ]",
				Name
			);
		} // LogUsage

		protected override void LogArgs() {
			base.LogArgs();

			Log.Msg("Specify include test customers as 'true' or 'false'.");
			Log.Msg("Specify calculation date in format 'yyyy-MM-dd'.");
		} // LogArgs
	} // class MedalVerificationInput
} // namespace
