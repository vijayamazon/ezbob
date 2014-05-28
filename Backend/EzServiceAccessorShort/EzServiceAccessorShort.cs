namespace EzServiceShortcut {
	using EzBob.Backend.Strategies.VatReturn;
	using EzServiceAccessor;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class EzServiceAccessorShort : IEzServiceAccessor {
		#region static constructor

		static EzServiceAccessorShort() {
			ms_oLock = new object();
			ms_oLog = new SafeLog();
			ms_oDB = null;
		} // static constructor

		#endregion static constructor

		#region method Set

		public static void Set(AConnection oDB, ASafeLog oLog) {
			lock (ms_oLock) {
				ms_oDB = oDB;
				ms_oLog = oLog ?? new SafeLog();
			} // lock
		} // Set

		#endregion method Set

		#region public

		#region method CalculateVatReturnSummary

		public void CalculateVatReturnSummary(int nCustomerMarketplaceID) {
			var stra = new CalculateVatReturnSummary(nCustomerMarketplaceID, ms_oDB, ms_oLog);
			stra.Execute();
		} // CalculateVatReturnSummary

		#endregion method CalculateVatReturnSummary

		#endregion public

		#region private

		private static ASafeLog ms_oLog;
		private static AConnection ms_oDB;
		private static readonly object ms_oLock;

		#endregion private
	} // class EzServiceAccessorShort
} // namespace EzServiceShortcut
