namespace EzBob.Backend.Strategies.Admin {
	using System.Threading;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Exceptions;

	#region class Nop

	public class Nop : AStrategy {
		#region public

		#region constructor

		public Nop(int nLengthInSeconds, string sMsg, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_sMsg = (sMsg ?? string.Empty).Trim();
			m_nLengthInSeconds = nLengthInSeconds;

			if (nLengthInSeconds < 1)
				throw new Warning(Log, "Nop length is less than 1 second.");
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Nop - no operation"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Log.Msg("Nop({0}, {1}) method asleeper started...", m_nLengthInSeconds, m_sMsg);

			for (int i = 1; i <= m_nLengthInSeconds; i++) {
				Log.Msg("Nop({0}, {2}) method asleeper: {1}...", m_nLengthInSeconds, i, m_sMsg);
				Thread.Sleep(1000);
			} // for

			Log.Msg("Nop({0}, {1}) method asleeper complete.", m_nLengthInSeconds, m_sMsg);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly int m_nLengthInSeconds;
		private readonly string m_sMsg;

		#endregion private
	} // class Nop

	#endregion class Nop

	#region class Noop

	public class Noop : Nop {
		#region constructor

		public Noop(AConnection oDB, ASafeLog oLog) : base(3, "NOOP: just a 3 seconds NOP", oDB, oLog) {} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Noop - no operation (for simulating parameterless strategy)"; }
		} // Name

		#endregion property Name
	} // class Noop

	#endregion class Noop
} // namespace
