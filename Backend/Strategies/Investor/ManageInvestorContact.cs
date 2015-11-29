namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Database;

	public class ManageInvestorContact : AStrategy {
		
		public ManageInvestorContact(InvestorContactModel contact) {
			this.contact = contact;
		}//ctor

		public override string Name { get { return "ManageInvestorContact"; } }

		public override void Execute() {
			DateTime now = DateTime.UtcNow;
			var con = DB.GetPersistent();
			con.BeginTransaction();
			
			try {
					var dbContact = new I_InvestorContact {
						InvestorContactID = this.contact.InvestorContactID,
						Email = this.contact.Email,
						InvestorID = this.contact.InvestorID,
						Comment = this.contact.Comment,
						LastName = this.contact.LastName,
						Mobile = this.contact.Mobile,
						OfficePhone = this.contact.OfficePhone,
						PersonalName = this.contact.PersonalName,
						Role = this.contact.Role,
						IsPrimary = this.contact.IsPrimary,
						Timestamp = now,
						IsActive = this.contact.IsActive,
					};

					DB.ExecuteNonQuery(con, this.contact.InvestorContactID == 0 ? "I_InvestorContactSave" : "I_InvestorContactUpdate", CommandSpecies.StoredProcedure,
						DB.CreateTableParameter<I_InvestorContact>("Tbl", new List<I_InvestorContact> { dbContact })
					);
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to save investor {0} contact to DB", this.contact.InvestorID);
				con.Rollback();
				Result = false;
				throw;
			}

			con.Commit();
			Result = true;
			Log.Info("Save investor {0} contact data into DB complete.", this.contact.InvestorID);
		}//Execute

		public bool Result { get; set; }

		private readonly InvestorContactModel contact;
	}//ManageInvestorContact
}//ns
