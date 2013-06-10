using System.Xml;

namespace Integration.ChannelGrabberAPI {
	#region interface IAccountData

	public interface IAccountData : IJsonable {
		string AccountTypeName();

		/// <summary>
		/// Verifies that Channel Grabber service returned valid data as a reply for
		/// account registration request.
		/// Stores account id internally (it can be retrieved by calling Id() later) if data
		/// is good.
		/// </summary>
		/// <exception cref="ChannelGrabberApiException">Throws in case of invalid data.</exception>
		/// <param name="doc">Data to check.</param>
		void VerifyRegistrationInProgress(XmlDocument doc);

		/// <summary>
		/// Verifies that this account does not exist in the list. Completes successfully if
		/// account not found.
		/// </summary>
		/// <exception cref="ChannelGrabberApiException">Throws if account exists.</exception>
		/// <param name="doc">List of accounts.</param>
		void VerifyNotExist(XmlDocument doc);

		/// <summary>
		/// Verifies that this account exists in the list. Stores account id internally if
		/// account found.
		/// </summary>
		/// <exception cref="ChannelGrabberApiException">Throws if account does not exist.</exception>
		/// <param name="doc">List of accounts.</param>
		void VerifyAccountID(XmlDocument doc);

		/// <summary>
		/// Returns account id retrieved by the last VerifyRegistationInProgress call.
		/// </summary>
		/// <returns>Account id.</returns>
		int Id();

		/// <summary>
		/// Checks that supplied credentials are ok and shop has been registered successfully.
		/// </summary>
		/// <param name="doc">Service output for validity request.</param>
		void Validate(XmlDocument doc);
	} // IAccountData

	#endregion interface IAccountData
} // namespace Integration.ChannelGrabberAPI
