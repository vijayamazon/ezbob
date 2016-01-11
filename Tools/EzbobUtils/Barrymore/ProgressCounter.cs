namespace Ezbob.Utils {
	using Ezbob.Logger;

	public class ProgressCounter {
		public ProgressCounter(string sFormat, ASafeLog oLog = null, ulong nCheckpoint = 1000, Severity nSeverity = 0) {
			this.log = oLog.Safe();
			this.counter = 0;
			this.thousands = 0;
			this.checkpoint = nCheckpoint;
			this.severity = nSeverity;
			this.format = sFormat ?? "";
		} // constructor

		public static ProgressCounter operator ++(ProgressCounter pc) {
			return pc.Next();
		} // operator ++

		public ProgressCounter Next() {
			Increment();
			return this;
		} // Next

		public bool Increment() {
			bool say = false;
			ulong sayWhat = 0;

			lock (locker) {
				++this.counter;

				if (this.counter == this.checkpoint) {
					this.thousands += this.counter;
					this.counter = 0;

					say = true;
					sayWhat = this.thousands;
				} // if
			} // lock

			if (say)
				this.log.Say(this.severity, this.format, sayWhat);

			return say;
		} // Increment

		public void Log() {
			if (this.counter == 0) {
				if (this.thousands == 0)
					this.log.Say(this.severity, this.format, 0);
			}
			else
				this.log.Say(this.severity, this.format, this.thousands + this.counter);
		} // Log

		public ulong CurrentPosition {
			get { return this.counter + this.thousands; }
		} // CurrentPosition

		private ulong counter;
		private ulong thousands;
		private readonly ulong checkpoint;
		private readonly Severity severity;
		private readonly string format;
		private readonly ASafeLog log;

		private static readonly object locker = new object();
	} // class ProgressCounter
} // namespace
