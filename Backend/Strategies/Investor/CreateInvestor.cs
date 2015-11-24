﻿namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using System.Collections.Generic;
	using System.Web.Security;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Backend.Strategies.UserManagement;
	using Ezbob.Database;

	public class CreateInvestor : AStrategy {
		public CreateInvestor(InvestorModel investor, IEnumerable<InvestorContactModel> contacts, IEnumerable<InvestorBankAccountModel> banks) {
			this.investor = investor;
			this.contacts = contacts;
			this.banks = banks;
		}

		public override string Name { get { return "CreateInvestor"; } }

		public override void Execute() {
			DateTime now = DateTime.UtcNow;
			var con = DB.GetPersistent();
			con.BeginTransaction();
			
			try {
				int investorID = DB.ExecuteScalar<int>(con, "I_InvestorSave", CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<I_Investor>("Tbl",
						new List<I_Investor> {
							new I_Investor {
								Name = this.investor.Name,
								InvestorTypeID = this.investor.InvestorTypeID,
								IsActive = true,
								Timestamp = now
							}
						})
					);

				if (investorID == 0) {
					throw new StrategyWarning(this, "Failed creating investor");
				}

				foreach (var contact in this.contacts) {
					UserSignup userSingup = new UserSignup(contact.Email, "123456", "InvestorWeb", 2);
					userSingup.ConnectionWrapper = con;
					userSingup.Execute();

					if (!userSingup.Status.HasValue || userSingup.Status.Value != MembershipCreateStatus.Success || userSingup.UserID <= 0) {
						throw new StrategyWarning(this, userSingup.Result);
					}

					var dbContact = new I_InvestorContact {
						InvestorContactID = userSingup.UserID,
						Timestamp = now,
						IsActive = true,
						Email = contact.Email,
						InvestorID = investorID,
						Comment = contact.Comment,
						IsPrimary = true,
						LastName = contact.LastName,
						Mobile = contact.Mobile,
						OfficePhone = contact.OfficePhone,
						PersonalName = contact.PersonalName,
						Role = contact.Role
					};

					int contactID = DB.ExecuteScalar<int>(con, "I_InvestorContactSave", CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<I_InvestorContact>("Tbl", new List<I_InvestorContact> { dbContact }));

					if (contactID == 0) {
						throw new StrategyWarning(this, "Failed creating investor contact");
					}
				}

				var dbBanks = new List<I_InvestorBankAccount>();
				foreach (var bank in this.banks) {
					dbBanks.Add(new I_InvestorBankAccount {
						IsActive = true,
						Timestamp = now,
						BankAccountNumber = bank.BankAccountNumber,
						BankAccountName = bank.BankAccountName,
						BankBranchName = bank.BankBranchName,
						BankBranchNumber = bank.BankBranchNumber,
						BankCode = bank.BankCode,
						BankCountryID = bank.BankCountryID,
						BankName = bank.BankName,
						InvestorAccountTypeID = bank.InvestorAccountTypeID,
						InvestorBankAccountID = bank.InvestorBankAccountID,
						RepaymentKey = bank.RepaymentKey,
						InvestorID = investorID
					});
				}

				DB.ExecuteNonQuery(con, "I_InvestorBankAccountSave", CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<I_InvestorBankAccount>("Tbl", dbBanks)
					);
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to save investor to DB");
				con.Rollback();
				Result = false;
				throw;
			}

			con.Commit();
			Result = true;
			Log.Info("Save investor data into DB complete.");
		}

		public bool Result { get; set; }

		private readonly InvestorModel investor;
		private readonly IEnumerable<InvestorContactModel> contacts;
		private readonly IEnumerable<InvestorBankAccountModel> banks;
	}
}
