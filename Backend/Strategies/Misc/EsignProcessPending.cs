namespace EzBob.Backend.Strategies.Misc {
	using EchoSignLib;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class EsignProcessPending : AStrategy {
		#region public

		#region constructor

		public EsignProcessPending(int? nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
			m_oFacade = new EchoSignFacade(DB, Log);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "EsignProcessPending"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oFacade.ProcessPending(m_nCustomerID);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly EchoSignFacade m_oFacade;
		private readonly int? m_nCustomerID;

		#endregion private
	} // class EsignProcessPending
} // namespace
