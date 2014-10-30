namespace EzBob.Backend.Strategies.Reports {
	using Ezbob.Database;
	using Ezbob.Logger;
	using global::Reports;

	public class Alibaba : AStrategy {
		#region public

		#region constructor

		public Alibaba(AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog) {
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Alibaba"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private Report m_oDataSharing;
		private Report m_oFunnel;

		#endregion private
	} // class Alibaba
} // namespace
