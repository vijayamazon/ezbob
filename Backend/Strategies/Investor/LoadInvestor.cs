namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Database;

	public class LoadInvestor : AStrategy {

		public LoadInvestor(int investorID) {
			this.investorID = investorID;
		}//ctor

		public override string Name { get { return "LoadInvestor"; } }

		public override void Execute() {
			var dbInvestor = LoadFromDb();
			Result = ContvertToModel(dbInvestor);
			Log.Info("Load investor {0} data from DB complete.", this.investorID);
		}//Execute

		private I_Investor LoadFromDb() {
			try {
				var data = DB.ExecuteEnumerable(
					"I_InvestorLoadFull",
					CommandSpecies.StoredProcedure,
					new QueryParameter("InvestorID", this.investorID)
				);

				var investor = new I_Investor();
				var accountTypes = new List<I_InvestorAccountType>();

				foreach (SafeReader sr in data) {
					string sType = sr["DatumType"];

					switch (sType) {
						case "InvestorData":
							sr.Fill(investor);
							break;

						case "InvestorContactData":
							var contact = sr.Fill<I_InvestorContact>();
							investor.InvestorContacts.Add(contact);
							break;

						case "InvestorBankAccountData":
							var bank = sr.Fill<I_InvestorBankAccount>();
							investor.InvestorBankAccounts.Add(bank);
							break;

						case "InvestorTypeData":
							var type = sr.Fill<I_InvestorType>();
							investor.InvestorType = type;
							break;

						case "InvestorAccountTypeData":
							var accountType = sr.Fill<I_InvestorAccountType>();
							accountTypes.Add(accountType);
							break;
					} // switch
				} // for each row

				foreach (var bank in investor.InvestorBankAccounts) {
					bank.AccountType = accountTypes.FirstOrDefault(x => x.InvestorAccountTypeID == bank.InvestorAccountTypeID);
				} // for

				return investor;
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to load investor {0} from DB", this.investorID);
				throw;
			}//try
		}//LoadFromDb

		private InvestorModel ContvertToModel(I_Investor dbInvestor) {
			var model = new InvestorModel {
				InvestorID = dbInvestor.InvestorID,
				InvestorType = new InvestorTypeModel {
					InvestorTypeID = dbInvestor.InvestorType.InvestorTypeID,
					Name = dbInvestor.InvestorType.Name
				},
				Name = dbInvestor.Name,
				IsActive = dbInvestor.IsActive,
				Timestamp = dbInvestor.Timestamp,
				Contacts = new List<InvestorContactModel>(),
				Banks = new List<InvestorBankAccountModel>()
			};

			foreach (var contact in dbInvestor.InvestorContacts) {
				model.Contacts.Add(new InvestorContactModel {
					InvestorID = contact.InvestorID,
					IsActive = contact.IsActive,
                    IsGettingAlerts = contact.IsGettingAlerts,
                    IsGettingReports = contact.IsGettingReports,
					Timestamp = contact.Timestamp,
					IsPrimary = contact.IsPrimary,
					Email = contact.Email,
					OfficePhone = contact.OfficePhone,
					PersonalName = contact.PersonalName,
					Mobile = contact.Mobile,
					Role = contact.Role,
					LastName = contact.LastName,
					InvestorContactID = contact.InvestorContactID,
					Comment = contact.Comment
				});
			}

			foreach (var bank in dbInvestor.InvestorBankAccounts) {
				model.Banks.Add(new InvestorBankAccountModel {
					InvestorID = bank.InvestorID,
					IsActive = bank.IsActive,
					Timestamp = bank.Timestamp,
					InvestorBankAccountID = bank.InvestorBankAccountID,
					BankAccountNumber = bank.BankAccountNumber,
					BankAccountName = bank.BankAccountName,
					BankCode = bank.BankCode,
					BankBranchNumber = bank.BankBranchNumber,
					RepaymentKey = bank.RepaymentKey,
					BankName = bank.BankName,
					BankBranchName = bank.BankBranchName,
					BankCountryID = bank.BankCountryID,
					AccountType = new InvestorAccountTypeModel {
						Name = bank.AccountType.Name,
						InvestorAccountTypeID = bank.AccountType.InvestorAccountTypeID
					}
				});
			}
			return model;
		}//ContvertToModel

		public InvestorModel Result { get; set; }

		private readonly int investorID;
	}//LoadInvestor
}//ns
