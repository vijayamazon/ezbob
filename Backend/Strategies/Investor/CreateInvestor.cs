namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Backend.Strategies.UserManagement;
	using Ezbob.Database;

	public class CreateInvestor : AStrategy {
        public CreateInvestor(int underwriterID, InvestorModel investor, IEnumerable<InvestorContactModel> contacts, IEnumerable<InvestorBankAccountModel> banks)
        {
              this.underwriterID = underwriterID;
			this.investor = investor;
			this.contacts = contacts;
			this.banks = banks;
		}//ctor

		public override string Name { get { return "CreateInvestor"; } }

		public override void Execute() {
			DateTime now = DateTime.UtcNow;
			var con = DB.GetPersistent();
			con.BeginTransaction();
			
			try {
				InvestorID = DB.ExecuteScalar<int>(con, "I_InvestorSave", CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<I_Investor>("Tbl",
						new List<I_Investor> {
							new I_Investor {
								Name = this.investor.Name,
								InvestorTypeID = this.investor.InvestorType.InvestorTypeID,
								IsActive = false,
								Timestamp = now,
								MonthlyFundingCapital = this.investor.MonthlyFundingCapital,
								FundingLimitForNotification = this.investor.FundingLimitForNotification,
								FundsTransferDate = this.investor.FundsTransferDate,
							}
						})
					);

				if (InvestorID == 0) {
					throw new StrategyWarning(this, "Failed creating investor");
				}

				foreach (var contact in this.contacts) {
					var userSingup = new SignupInvestorMultiOrigin(contact.Email);
					userSingup.Transaction = con;
					userSingup.Execute();

					// The .Execute() above completes successfully if and only if no error detected and .UserID > 0.

					var dbContact = new I_InvestorContact {
						InvestorContactID = userSingup.UserID,
						Email = contact.Email,
						InvestorID = InvestorID,
						Comment = contact.Comment,
						LastName = contact.LastName,
						Mobile = contact.Mobile,
						OfficePhone = contact.OfficePhone,
						PersonalName = contact.PersonalName,
						Role = contact.Role,
						IsPrimary = contact.IsPrimary,
						Timestamp = now,
						IsActive = true,
					};

					int contactsCount = DB.ExecuteScalar<int>(con, "I_InvestorContactSave", CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<I_InvestorContact>("Tbl", new List<I_InvestorContact> { dbContact }));

					if (contactsCount != 1) {
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
						InvestorAccountTypeID = bank.AccountType.InvestorAccountTypeID,
						InvestorBankAccountID = bank.InvestorBankAccountID,
						RepaymentKey = bank.RepaymentKey,
                        UserID = this.underwriterID,
						InvestorID = InvestorID
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
		}//Execute

		public bool Result { get; set; }
		public int InvestorID { get; private set; }
        private readonly int underwriterID;
		private readonly InvestorModel investor;
		private readonly IEnumerable<InvestorContactModel> contacts;
		private readonly IEnumerable<InvestorBankAccountModel> banks;
	}//CreateInvestor
}//ns
