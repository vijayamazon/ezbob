using System;
using Ezbob.HmrcHarvester;
using Ezbob.Logger;
using Integration.ChannelGrabberAPI;
using Integration.ChannelGrabberConfig;
using log4net;
using DBCustomer = EZBob.DatabaseLib.Model.Database.Customer;

namespace Integration.ChannelGrabberFrontend {
	#region class Connector

	public class Connector : IHarvester {
		#region public

		#region method SetBackdoorData

		public static void SetBackdoorData(string sAccountTypeName, int nCustomerMarketplaceID, Hopper oHopper) {
			VendorInfo vi = Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(sAccountTypeName);

			if (vi == null)
				return;

			switch (vi.Behaviour) {
			case Behaviour.Default:
				// nothing to do here
				break;

			case Behaviour.HMRC:
				Ezbob.HmrcHarvester.Harvester.SetBackdoorData(nCustomerMarketplaceID, oHopper);
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // SetBackdoorData

		#endregion method SetBackdoorData

		#region method SetRunningInWebEnvFlag

		public static void SetRunningInWebEnvFlag(string sAccountTypeName, int nCustomerMarketplaceID) {
			VendorInfo vi = Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(sAccountTypeName);

			if (vi == null)
				return;

			switch (vi.Behaviour) {
			case Behaviour.Default:
				// nothing to do here
				break;

			case Behaviour.HMRC:
				Ezbob.HmrcHarvester.Harvester.SetRunningInWebEnvFlag(nCustomerMarketplaceID);
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // SetRunningInWebEnvFlag

		#endregion method SetRunningInWebEnvFlag

		#region method FetchRunningInWebEnvFlag

		public static bool FetchRunningInWebEnvFlag(string sAccountTypeName, int nCustomerMarketplaceID, ASafeLog log) {
			VendorInfo vi = Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(sAccountTypeName);

			if (vi == null)
				return false;

			switch (vi.Behaviour) {
			case Behaviour.Default:
				return false;

			case Behaviour.HMRC:
				return Ezbob.HmrcHarvester.Harvester.FetchRunningInWebEnvFlag(nCustomerMarketplaceID, log);

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // FetchRunningInWebEnvFlag

		#endregion method FetchRunningInWebEnvFlag

		#region constructor

		public Connector(AccountData oAccountData, ILog log, DBCustomer oCustomer) {
			if (oAccountData == null)
				throw new ApiException("Account data not specified.");

			switch (oAccountData.VendorInfo.Behaviour) {
			case Behaviour.HMRC:
				DataHarvester = new Ezbob.HmrcHarvester.Harvester(oAccountData, log);
				break;

			case Behaviour.Default:
				DataHarvester = new Integration.ChannelGrabberAPI.Harvester(oAccountData, log, oCustomer);
				break;

			default:
				throw new ApiException("Unsupported behaviour for CG flavour: " + oAccountData.VendorInfo.Behaviour.ToString());
			} // switch
		} // constructor

		#endregion constructor

		#region method Init

		public virtual bool Init() {
			try {
				return DataHarvester.Init();
			}
			catch (Integration.ChannelGrabberAPI.ConnectionFailException cfe) {
				throw new ConnectionFailException(cfe.Message, cfe);
			}
			catch (Integration.ChannelGrabberAPI.ApiException ae) {
				throw new ApiException(ae.Message, ae);
			} // try
		} // Init

		#endregion method Init

		#region method Run

		public virtual void Run(bool bValidateCredentialsOnly) {
			try {
				DataHarvester.Run(bValidateCredentialsOnly);
			}
			catch (Integration.ChannelGrabberAPI.ConnectionFailException cfe) {
				throw new ConnectionFailException(cfe.Message, cfe);
			}
			catch (Integration.ChannelGrabberAPI.ApiException ae) {
				throw new ApiException(ae.Message, ae);
			} // try
		} // Run

		public virtual void Run(bool bValidateCredentialsOnly, int nCustomerMarketplaceID) {
			try {
				DataHarvester.Run(bValidateCredentialsOnly, nCustomerMarketplaceID);
			}
			catch (Integration.ChannelGrabberAPI.ConnectionFailException cfe) {
				throw new ConnectionFailException(cfe.Message, cfe);
			}
			catch (Integration.ChannelGrabberAPI.ApiException ae) {
				throw new ApiException(ae.Message, ae);
			} // try
		} // Run

		#endregion method Run

		#region method Done

		public virtual void Done() {
			try {
				DataHarvester.Done();
			}
			catch (Integration.ChannelGrabberAPI.ConnectionFailException cfe) {
				throw new ConnectionFailException(cfe.Message, cfe);
			}
			catch (Integration.ChannelGrabberAPI.ApiException ae) {
				throw new ApiException(ae.Message, ae);
			} // try
		} // Done

		#endregion method Done

		#region property DataHarvester

		public IHarvester DataHarvester { get; private set; }

		#endregion property DataHarvester

		#endregion public
	} // class Connector

	#endregion class Connector
} // namespace Integration.ChannelGrabberFrontend
