namespace Integration.ChannelGrabberAPI {
	#region interface IHarvester

	public interface IHarvester {
		bool Init();
		bool Run(bool bValidateCredentialsOnly);
		void Done();
	} // interface IHarvester

	#endregion interface IHarvester
} // namespace Integration.ChannelGrabberAPI
