namespace Ezbob.Backend.Strategies.UserManagement {
    using System;
    using System.Linq;
	using Ezbob.Database;
	using MailChimp;
	using MailChimp.Types;

    public class UserDisable : AStrategy {
        public UserDisable(
            int customerID,
			string email,
			bool unsubscribeFromMailChimp,
            bool changeEmail) {
            this.customerID = customerID;
            this.email = email;
            this.unsubscribeFromMailChimp = unsubscribeFromMailChimp;
            this.changeEmail = changeEmail;
        } // constructor

		public override string Name {
			get { return "User disable"; }
		} // Name

		public override void Execute() {
			Log.Debug("Disable User {0} {1} ", this.customerID, this.email);
            if (this.changeEmail) {
                DB.ExecuteNonQuery("CustomerEmailRenameFrozen", CommandSpecies.StoredProcedure, 
                    new QueryParameter("CustomerID", this.customerID));
            }

            if(this.unsubscribeFromMailChimp) {
                UnsubscribeFromAllLists();
            }
		} // Execute

        private void UnsubscribeFromAllLists() {
            try {
                MCApi mailChimpApiClient = new MCApi(ConfigManager.CurrentValues.Instance.MailChimpApiKey, true);
                MCList<string> lists = mailChimpApiClient.ListsForEmail(this.email);
                if (lists != null && lists.Any()) {
                    foreach (var list in lists) {
                        var options = new List.UnsubscribeOptions {
                            DeleteMember = true,
                            SendGoodby = false,
                            SendNotify = false
                        };
                        var optOptions = new Opt<List.UnsubscribeOptions>(options);
                        bool success = mailChimpApiClient.ListUnsubscribe(list, this.email, optOptions);
                        if (success) {
                            Log.Info("{0} was unsubscribed successfully from mail chimp", this.email);
                        }
                    }
                }
            } catch (MCException mcEx) {
                Log.Warn(mcEx.Error.ToString());
            } catch (Exception ex) {
                Log.Warn(ex, "Failed unsubscribe from mail chimp email {0} for customer {1}", this.email, this.customerID);
            }
        }

        private readonly int customerID;
        private readonly string email;
        private readonly bool unsubscribeFromMailChimp;
        private readonly bool changeEmail;

	} // class UserDisable
} // namespace Ezbob.Backend.Strategies.UserManagement
