﻿namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using System.Collections.Generic;
	using System.Web.Security;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Backend.Strategies.UserManagement;
	using Ezbob.Database;

	public class ManageInvestorContact : AStrategy {
		
		public ManageInvestorContact(InvestorContactModel contact) {
			this.contact = contact;
		}//ctor

		public override string Name { get { return "ManageInvestorContact"; } }

		public override void Execute() {
				throw new NotImplementedException("Thou shalt create SignupInvestorMultiOrigin strategy.");
				// TODO:
				// 1. Create SignupInvestorMultiOrigin
				// 2. Check current sign up/create lead ones for collissions with investors.
				// 3. Uncomment the code below.

			/* TODO Uncomment here once SignupInvestorMultiOrigin is ready
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

				if (this.contact.InvestorContactID == 0) {
					UserSignup userSingup = new UserSignup(this.contact.Email, "123456", "InvestorWeb", 2);
					userSingup.ConnectionWrapper = con;
					userSingup.Execute();

					if (!userSingup.Status.HasValue || userSingup.Status.Value != MembershipCreateStatus.Success || userSingup.UserID <= 0) {
						throw new StrategyWarning(this, userSingup.Result);
					}

					dbContact.InvestorContactID = userSingup.UserID;

					DB.ExecuteNonQuery(con, "I_InvestorContactSave", CommandSpecies.StoredProcedure,
						DB.CreateTableParameter<I_InvestorContact>("Tbl", new List<I_InvestorContact> { dbContact })
					);
				} else {
					DB.ExecuteNonQuery(con, "I_InvestorContactUpdate", CommandSpecies.StoredProcedure,
						DB.CreateTableParameter<I_InvestorContact>("Tbl", new List<I_InvestorContact> { dbContact })
					);
				}
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to save investor {0} contact to DB", this.contact.InvestorID);
				con.Rollback();
				Result = false;
				throw;
			}

			con.Commit();
			Result = true;
			Log.Info("Save investor {0} contact data into DB complete.", this.contact.InvestorID);
			*/
		}//Execute

		public bool Result { get; set; }

		private readonly InvestorContactModel contact;
	}//ManageInvestorContact
}//ns
