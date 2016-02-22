
using System;
using System.Collections.Generic;
using Ezbob.Backend.ModelsWithDB.OpenPlatform;
using Ezbob.Database;

namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Database;

	public class SaveInvestorContactList : AStrategy {
		public SaveInvestorContactList(int investorID, IEnumerable<InvestorContactModel> contacts) {
			this.investorID = investorID;
			this.contacts = contacts;
		}//ctor

		public override string Name { get { return "SaveInvestorContactList"; } }

		public override void Execute() {
			DateTime now = DateTime.UtcNow;
			try {
				var dbContacts = new List<I_InvestorContact>();
				foreach (var contact in this.contacts) {
					dbContacts.Add(new I_InvestorContact {
						InvestorContactID = contact.InvestorContactID,
						InvestorID = this.investorID,
						PersonalName = contact.PersonalName,
						LastName = contact.LastName,
						Email = contact.Email,
						Role = contact.Role,
						Comment = contact.Comment,
						IsPrimary = contact.IsPrimary,
						Mobile = contact.Mobile,
						OfficePhone = contact.OfficePhone,
						IsActive = contact.IsActive,
                        IsGettingAlerts = contact.IsGettingAlerts,
                        IsGettingReports = contact.IsGettingReports,
						Timestamp = now,
					});
				}

				DB.ExecuteNonQuery("I_InvestorEditContactList", CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<I_InvestorContact>("Tbl", dbContacts)
					);
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to edit investor contract on DB");

				Result = false;
				throw;
			}//try

			Result = true;
		}//Execute

		public bool Result { get; set; }
		
		private readonly int investorID;
		private readonly IEnumerable<InvestorContactModel> contacts;
	}//SaveInvestorContactList
}//ns
