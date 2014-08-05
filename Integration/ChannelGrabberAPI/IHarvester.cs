namespace Integration.ChannelGrabberAPI {
	using System.Collections.Generic;

	public interface IHarvester {
		bool Init();
		void Run(bool bValidateCredentialsOnly);
		void Run(bool bValidateCredentialsOnly, int nCustomerMarketplaceID);
		void Done();

		int SourceID { get; }

		SortedDictionary<string, string> ErrorsToEmail { get; }
	} // interface IHarvester
} // namespace Integration.ChannelGrabberAPI
