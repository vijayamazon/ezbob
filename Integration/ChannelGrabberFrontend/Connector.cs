using Integration.ChannelGrabberAPI;
using Integration.ChannelGrabberConfig;
using log4net;
using DBCustomer = EZBob.DatabaseLib.Model.Database.Customer;

namespace Integration.ChannelGrabberFrontend {
	#region class Connector

	public class Connector : IHarvester {
		#region public

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
				break;
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

		public virtual bool Run(bool bValidateCredentialsOnly) {
			try {
				return DataHarvester.Run(bValidateCredentialsOnly);
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
