namespace Ezbob.Backend.Strategies.VatReturn {
	using System;
	using global::Integration.ChannelGrabberConfig;
	using global::Integration.ChannelGrabberFrontend;
	using MailStrategies;

	public class ValidateAndUpdateLinkedHmrcPassword : UpdateLinkedHmrcPassword {
		public ValidateAndUpdateLinkedHmrcPassword(
			string sCustomerID,
			string sDisplayName,
			string sPassword
		) : base(sCustomerID, sDisplayName, sPassword) {
			ErrorMessage = null;
		} // constructor

		public override string Name {
			get { return "ValidateAndUpdateLinkedHmrcPassword"; }
		} // Name

		public override void Execute() {
			AccountData data = ValidateInput();

			if (data == null) {
				Log.Warn("Failed to validate credentials for linked HMRC account.");
				return;
			} // if

			try {
				m_oCustomerData = new CustomerData(this, CustomerID, DB);
				m_oCustomerData.Load();
			}
			catch (Exception e) {
				Log.Warn(
					e,
					"Failed to validate credentials for linked HMRC account {0} ({1}).",
					data.CustomerMarketplaceID,
					data.SecInfo.login
				);
				ErrorMessage = "Failed to validate credentials. Please retry.";
				return;
			} // try

			if (CheckHmrc(data))
				UpdatePassword(data);
		} // Execute

		public string ErrorMessage { get; private set; }

		private CustomerData m_oCustomerData;

		private bool CheckHmrc(AccountData data) {
			if ((data.SecInfo.login == m_oCustomerData.Mail) && (data.SecInfo.password == VendorInfo.TopSecret)) {
				ErrorMessage = "Cannot update this account.";
				return false;
			} // if

			data.SecInfo.password = Password;

			try {
				var ctr = new Connector(data.SecInfo.Fill(), Log, m_oCustomerData.Id, m_oCustomerData.Mail);

				if (ctr.Init()) {
					ctr.Run(true);
					ctr.Done();
				} // if
			} catch (InvalidCredentialsException) {
				Log.Debug(
					"Invalid credentials detected for linked HMRC account {0} ({1}).",
					data.CustomerMarketplaceID,
					data.SecInfo.login
				);
				ErrorMessage = "Invalid user name or password.";
				return false;
			} catch (Exception e) {
				Log.Warn(
					e,
					"Failed to validate credentials for linked HMRC account {0} ({1}).",
					data.CustomerMarketplaceID,
					data.SecInfo.login
				);
				ErrorMessage = "Failed to validate credentials. Please retry.";
				return false;
			} // try

			return true;
		} // CheckHmrc
	} // class ValidateAndUpdateLinkedHmrcPassword
} // namespace
