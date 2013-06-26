using System;
using System.Collections.Generic;
using EzBob.CommonLib;
using Integration.ChannelGrabberConfig;
using log4net;

namespace Integration.ChannelGrabberFrontend {
	public class ServiceInfo : VendorInfo, IMarketplaceServiceInfo {
		public Guid InternalId { get { return Guid(); } }

		public ServiceInfo(string sAccountTypeName) {
			ms_oLog.DebugFormat("Constructing ServiceInfo for {0}", sAccountTypeName);

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

				ms_oLog.DebugFormat("Valid name: {0}", sAccountTypeName);
			} // if

			ms_oLog.DebugFormat("Constructed ServiceInfo for {0}", sAccountTypeName);
		} // constructor

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(RetriveDataHelper));
	} // ServiceInfo
} // namespace Integration.ChannelGrabberFrontend
