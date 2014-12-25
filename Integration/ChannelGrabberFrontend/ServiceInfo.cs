namespace Integration.ChannelGrabberFrontend {
	using System;
	using EzBob.CommonLib;
	using Integration.ChannelGrabberConfig;

	public class ServiceInfo : VendorInfo, IMarketplaceServiceInfo {
		public Guid InternalId { get { return Guid(); } }

		public ServiceInfo(string sAccountTypeName) {
			VendorInfo vi = Configuration.Instance.GetVendorInfo(sAccountTypeName);

			if (vi == null)
				return;

			Name = (string)vi.Name.Clone();
			DisplayName = (string)vi.DisplayName.Clone();
			Description = (string)vi.Description.Clone();
			InternalID = (string)vi.InternalID.Clone();

			SecurityData = (SecurityData)vi.SecurityData.Clone();

			ClientSide = (ClientSide)vi.ClientSide.Clone();

			SetGuid(new Guid(vi.Guid().ToString()));
			IsPaymentAccount = vi.HasExpenses;
		} // constructor
	} // ServiceInfo
} // namespace Integration.ChannelGrabberFrontend
