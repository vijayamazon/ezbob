namespace StrategiesActivator {
	using System.Collections.Generic;
	using Ezbob.Logger;

	internal class VerificationInput {
		public virtual bool IsGood {
			get { return this.isGood; }
			protected set { this.isGood = value; }
		} // IsGood

		public virtual int CustomerCount { get; private set; }
		public virtual int LastCheckedCustomerID { get; private set; }

		public VerificationInput(string sName, string[] args, ASafeLog oLog) {
			CustomerCount = -1;
			LastCheckedCustomerID = -1;

			this.isGood = false;

			Name = sName;
			Args = new Queue<string>(args);
			Log = oLog ?? new SafeLog();
		} // constructor

		public virtual void Init() {
			ProcessArgs();

			if (!IsGood)
				LogNotGood();
		} // Init

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"good: {2}, top: {0}, last: {1}",
				CustomerCount,
				LastCheckedCustomerID,
				IsGood
			);
		} // ToString

		protected virtual void ProcessArgs() {
			Args.Dequeue(); // skip strategy name

			IsGood = true;
			CustomerCount = -1;
			LastCheckedCustomerID = -1;

			if (Args.Count == 0)
				return;

			int n;

			IsGood = int.TryParse(Args.Dequeue(), out n);
			if (IsGood)
				CustomerCount = n;
			else
				return;

			if (Args.Count == 0)
				return;

			IsGood = int.TryParse(Args.Dequeue(), out n);
			if (IsGood)
				LastCheckedCustomerID = n;
		} // ProcessArgs

		protected virtual void LogNotGood() {
			LogUsage();
			LogArgs();
		} // LogNotGood

		protected virtual void LogUsage() {
			Log.Msg("Usage: {0} [ <count>  [ <last checked id> ] ]", Name);
		} // LogUsage

		protected virtual void LogArgs() {
			Log.Msg("Specify count 0 or negative to run on all the cases.");
			Log.Msg("Specify last checked id 0 or negative to start from the beginning.");
		} // LogArgs

		protected virtual string Name { get; private set; }
		protected virtual Queue<string> Args { get; private set; }
		protected virtual ASafeLog Log { get; private set; }

		private bool isGood;
	} // class VerificationInput
} // namespace
