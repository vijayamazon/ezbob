using System;
using System.Collections.Generic;
using Integration.ChannelGrabberConfig;

namespace Integration.ChannelGrabberFrontend {
	public class ServiceInfo : VendorInfo {
		public Guid InternalId { get { return Guid(); } }

		protected void Load(string sAccountTypeName) {
			VendorInfo vi = Configuration.Instance.GetVendorInfo(sAccountTypeName);

			if (vi != null) {
				Name = (string)vi.Name.Clone();
				DisplayName = (string)vi.DisplayName.Clone();
				Description = (string)vi.Description.Clone();
				InternalID = (string)vi.InternalID.Clone();

				SecurityData = (SecurityData)vi.SecurityData.Clone();

				Aggregators = new List<AggregatorInfo>();

				foreach (AggregatorInfo ai in vi.Aggregators)
					Aggregators.Add((AggregatorInfo)ai.Clone());

				ClientSide = (ClientSide)vi.ClientSide.Clone();

				SetGuid(new Guid(vi.Guid().ToString()));
			} // if
		} // Load
	} // ServiceInfo
} // namespace Integration.ChannelGrabberFrontend
