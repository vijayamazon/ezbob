namespace Ezbob.Backend.Strategies.Misc {
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;
	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;
	using global::Integration.ChannelGrabberConfig;
	using global::Integration.ChannelGrabberFrontend;

	public class UpdateUploadedHmrcDisplayName : AStrategy {
		public UpdateUploadedHmrcDisplayName(int customerID) {
			this.customerID = customerID;
		} // constructor

		public override string Name {
			get { return "UpdateUploadedHmrcDisplayName"; }
		} // Name

		public override void Execute() {
			Log.Debug("Loading uploaded HMRC data for customer {0}...", this.customerID);

			List<MpData> lst = DB.Fill<MpData>(
				"LoadCustomerUploadedHmrcAccountData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", this.customerID)
			);

			if (lst.Count < 1) {
				Log.Debug("Customer {0} has no uploaded HMRC marketplace.", this.customerID);
				return;
			} // if
			
			Log.Say(
				lst.Count > 1 ? Severity.Alert : Severity.Debug,
				"Customer {0} has {1}.",
				this.customerID,
				Grammar.Number(lst.Count, "uploaded HMRC marketplace")
			);

			var vendorInfo = global::Integration.ChannelGrabberConfig.Configuration.Instance.Hmrc;

			int counter = 1;

			foreach (MpData mpData in lst) {
				try {
					AccountModel x = Serialized.Deserialize<AccountModel>(Encrypted.Decrypt(mpData.SecurityData));

					if (x.password != VendorInfo.TopSecret) {
						Log.Warn(
							"Password is not '{1}' for marketplace {0}, skipping it.",
							mpData.MpID,
							VendorInfo.TopSecret
						);

						continue;
					} // if
				} catch {
					Log.Warn("Failed to deserialise security data for marketplace {0}, skipping it.", mpData.MpID);
					continue;
				} // try

				var model = new AccountModel {
					accountTypeName = vendorInfo.Name,
					displayName = mpData.NewEmail,
					name = mpData.NewEmail,
					login = mpData.NewEmail,
					password = VendorInfo.TopSecret,
				};

				string displayName = string.Format("{0}{1}", mpData.NewEmail, lst.Count > 1 ? "-" + counter : string.Empty);

				Log.Debug("Updating security data and display name for marketplace id {0}...", mpData.MpID);

				DB.ExecuteNonQuery(
					"UpdateCustomerUploadedHmrcAccountData",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@MpID", mpData.MpID),
					new QueryParameter("@DisplayName", displayName),
					new QueryParameter("@SecurityData", new Encrypted(new Serialized(model)))
				);

				Log.Debug("Updated security data and display name for marketplace id {0}.", mpData.MpID);

				counter++;
			} // for each
		} // Execute

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		private class MpData {
			public int MpID { get; set; }
			public byte[] SecurityData { get; set; }
			public string NewEmail { get; set; }
		} // class MpData

		private readonly int customerID;
	} // class UpdateUploadedHmrcDisplayName
} // namespace
