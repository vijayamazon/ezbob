namespace Ezbob.Backend.Strategies.Admin {
	using System.Threading;
	using Ezbob.Utils.Exceptions;

	public class Nop : AStrategy {

		public Nop(int nLengthInSeconds, string sMsg) {
			m_sMsg = (sMsg ?? string.Empty).Trim();
			m_nLengthInSeconds = nLengthInSeconds;

			if (nLengthInSeconds < 1)
				throw new Warning(Log, "Nop length is less than 1 second.");
		} // constructor

		public override string Name {
			get { return "Nop - no operation"; }
		} // Name

		public override void Execute() {
			Log.Msg("Nop({0}, {1}) method asleeper started...", m_nLengthInSeconds, m_sMsg);

			for (int i = 1; i <= m_nLengthInSeconds; i++) {
				Log.Msg("Nop({0}, {2}) method asleeper: {1}...", m_nLengthInSeconds, i, m_sMsg);
				Thread.Sleep(1000);
			} // for

			Log.Msg("Nop({0}, {1}) method asleeper complete.", m_nLengthInSeconds, m_sMsg);
		} // Execute

		private readonly int m_nLengthInSeconds;
		private readonly string m_sMsg;
	} // class Nop

	public class Noop : Nop {
		public Noop() : base(3, "NOOP: just a 3 seconds NOP") {} // constructor

		public override string Name {
			get { return "Noop - no operation (for simulating parameterless strategy)"; }
		} // Name
	} // class Noop
} // namespace
