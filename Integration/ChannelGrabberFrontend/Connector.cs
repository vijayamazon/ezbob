﻿namespace Integration.ChannelGrabberFrontend {
	using System;
	using System.Collections.Generic;
	using Ezbob.HmrcHarvester;
	using Ezbob.Logger;
	using Integration.ChannelGrabberAPI;
	using Integration.ChannelGrabberConfig;

	public class Connector : IHarvester {

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

		public Connector(AccountData oAccountData, ASafeLog log, int nCustomerID, string sCustomerEmail) {
			if (oAccountData == null)
				throw new ApiException("Account data not specified.");

			switch (oAccountData.VendorInfo.Behaviour) {
			case Behaviour.HMRC:
				DataHarvester = new Ezbob.HmrcHarvester.Harvester(oAccountData, log);
				break;

			case Behaviour.Default:
				DataHarvester = new Integration.ChannelGrabberAPI.Harvester(oAccountData, log, nCustomerID, sCustomerEmail);
				break;

			default:
				throw new ApiException("Unsupported behaviour for CG flavour: " + oAccountData.VendorInfo.Behaviour.ToString());
			} // switch
		} // constructor

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

		public IHarvester DataHarvester { get; private set; }

		public int SourceID {
			get { return DataHarvester.SourceID; } // get
		} // SourceID

		public SortedDictionary<string, string> ErrorsToEmail {
			get { return DataHarvester.ErrorsToEmail; }
			private set { }
		} // ErrorsToEmail

	} // class Connector
} // namespace Integration.ChannelGrabberFrontend
