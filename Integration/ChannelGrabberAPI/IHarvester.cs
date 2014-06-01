namespace Integration.ChannelGrabberAPI {
	#region interface IHarvester

	public interface IHarvester {
		bool Init();
		void Run(bool bValidateCredentialsOnly);
		void Run(bool bValidateCredentialsOnly, int nCustomerMarketplaceID);
		void Done();

		int SourceID { get; }
	} // interface IHarvester

	#endregion interface IHarvester
} // namespace Integration.ChannelGrabberAPI
