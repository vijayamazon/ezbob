namespace Ezbob.Backend.Strategies.Investor {
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Database;

	public class NotifyInvestorUtilizedFunds : AStrategy {
		public NotifyInvestorUtilizedFunds(int investorID) {
			this.investorID = investorID;
		}

		public override string Name { get { return "NotifyInvestorUtilizedFunds"; } }

		public override void Execute() {
			this.investor = DB.FillFirst<I_Investor>(string.Format("SELECT * FROM I_Investor WHERE InvestorID = {0}", this.investorID), CommandSpecies.Text);
			this.fundingBankAccount = DB.FillFirst<I_InvestorBankAccount>(string.Format("SELECT TOP 1 * FROM I_InvestorBankAccount WHERE InvestorID = {0} AND IsActive = 1 AND InvestorAccountTypeID = {1}", 
				this.investorID, 
				(int)I_InvestorAccountTypeEnum.Funding), 
				CommandSpecies.Text);

			this.systemBalance = DB.FillFirst<I_InvestorSystemBalance>(string.Format("SELECT TOP 1 * FROM I_InvestorSystemBalance WHERE InvestorID = {0} AND InvestorBankAccountID = {1} ORDER BY InvestorSystemBalanceID DESC",
				this.investorID, 
				this.fundingBankAccount.InvestorBankAccountID));

			if (!this.systemBalance.NewBalance.HasValue) {
				Log.Info("NotifyInvestorUtilizedFunds investor {0} has no balance in his funding account {1}",
					this.investorID, 
					this.fundingBankAccount.InvestorBankAccountID);
				return;
			}

			if (this.systemBalance.NewBalance.Value < ConfigManager.CurrentValues.Instance.MinLoan) {
				SendFundsUtilized();
			} else if (this.investor.MonthlyFundingCapital.HasValue && 
				this.investor.MonthlyFundingCapital.Value > 0 && 
				this.systemBalance.NewBalance.Value / this.investor.MonthlyFundingCapital.Value < 0.9M) {
				SendFundsUtilized();
			} else if (this.investor.MonthlyFundingCapital.HasValue && 
				this.investor.MonthlyFundingCapital.Value > 0 && 
				this.systemBalance.NewBalance.Value / this.investor.MonthlyFundingCapital.Value < 0.75M) {
				SendFundsUtilized();
			} else if (this.investor.FundingLimitForNotification.HasValue && 
				this.systemBalance.NewBalance.Value < this.investor.FundingLimitForNotification.Value) {
				SendFundsUtilized();
			}
		}

		private void SendFundsUtilized() {
			if (!this.systemBalance.NewBalance.HasValue) {
				return;
			}

			var contacts = DB.Fill<I_InvestorContact>(string.Format("SELECT * FROM I_InvestorContact WHERE InvestorID = {0} AND IsActive = 1", 
				this.investorID),
				CommandSpecies.Text);
			

			foreach (var investorContact in contacts) {
				StrategiesMailer mailer = new StrategiesMailer();
				
				Dictionary<string, string> parameters = new Dictionary<string, string> {
					{ "ContactFirstName", investorContact.PersonalName },
					{ "ContactLastName", investorContact.LastName },
					{ "InvestorName", this.investor.Name },
					{ "CurrentBalace", this.systemBalance.NewBalance.Value.ToString("#,#") },
					{ "CurrentBalaceDate", this.systemBalance.Timestamp.ToString("dd/MM/yy hh:mm") },
					{ "BankAccountName", this.fundingBankAccount.BankAccountName },
					{ "BankAccountNumber", this.fundingBankAccount.BankAccountNumber },
				};

				mailer.Send("InvestorUtilizedFunds", 
					parameters, 
					new Addressee(
						investorContact.Email, 
						bShouldRegister: true, 
						userID: investorContact.InvestorContactID,
						addSalesforceActivity: false)
				);
			}
		}

		private readonly int investorID;
		private I_Investor investor;
		private I_InvestorBankAccount fundingBankAccount;
		private I_InvestorSystemBalance systemBalance;
	}
}
