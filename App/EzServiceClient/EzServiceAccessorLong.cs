namespace ServiceClientProxy {
	using EzServiceAccessor;

	public class EzServiceAccessorLong : IEzServiceAccessor {
		#region public

		#region constructor

		public EzServiceAccessorLong() {
			m_oServiceClient = new ServiceClient();
		} // constructor

		#endregion constructor

		#region method CalculateVatReturnSummary

		public void CalculateVatReturnSummary(int nCustomerMarketplaceID) {
			m_oServiceClient.Instance.CalculateVatReturnSummary(nCustomerMarketplaceID);
		} // CalculateVatReturnSummary

		#endregion method CalculateVatReturnSummary

		#endregion public

		#region private

		private readonly ServiceClient m_oServiceClient;

		#endregion private
	} // class EzServiceAccessorLong
} // namespace ServiceClientProxy
