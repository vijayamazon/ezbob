namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Database;

	public class NotifyInvestorUtilizedFunds : AStrategy {
		public NotifyInvestorUtilizedFunds(int investorID) {
			this.investorID = investorID;
		}//ctor

		public override string Name { get { return "NotifyInvestorUtilizedFunds"; } }

		public override void Execute() {
			try {
				//get investor
				this.investor = DB.FillFirst<I_Investor>(
					string.Format("SELECT * FROM I_Investor WHERE InvestorID = {0}", this.investorID), CommandSpecies.Text);
				
				//get accounting configuration
				this.investorAccountingConfiguration = DB.FillFirst<I_InvestorAccountingConfiguration>(
					string.Format("SELECT * FROM I_Investor WHERE InvestorID = {0}", this.investorID), CommandSpecies.Text);

				//get investor's funding account
				this.fundingBankAccount = DB.FillFirst<I_InvestorBankAccount>(
					string.Format("SELECT TOP 1 * FROM I_InvestorBankAccount WHERE InvestorID = {0} AND IsActive = 1 AND InvestorAccountTypeID = {1}",
					this.investorID,
					(int)I_InvestorAccountTypeEnum.Funding),
					CommandSpecies.Text);

				//get investor's system balance
				this.systemBalance = DB.FillFirst<I_InvestorSystemBalance>(
					string.Format("SELECT TOP 1 * FROM I_InvestorSystemBalance WHERE InvestorBankAccountID = {0} ORDER BY InvestorSystemBalanceID DESC",
					this.fundingBankAccount.InvestorBankAccountID));

				if (!this.systemBalance.NewBalance.HasValue) {
					Log.Info("NotifyInvestorUtilizedFunds investor {0} has no balance in his funding account {1}",
						this.investorID,
						this.fundingBankAccount.InvestorBankAccountID);
					return;
				}//if

				if (this.systemBalance.NewBalance.Value < CurrentValues.Instance.MinLoan) {
					Log.Info("NotifyInvestorUtilizedFunds investor {0} balance less then min loan {1} {2}",
							this.investorID,
							this.systemBalance.NewBalance,
							CurrentValues.Instance.MinLoan);
					SendFundsUtilized();
				} else if (this.investorAccountingConfiguration.MonthlyFundingCapital.HasValue &&
					this.investorAccountingConfiguration.MonthlyFundingCapital.Value > 0 &&
					this.systemBalance.NewBalance.Value / this.investorAccountingConfiguration.MonthlyFundingCapital.Value < CurrentValues.Instance.InvestorFundsUtilized75) {
						Log.Info("NotifyInvestorUtilizedFunds investor {0} balance less then 75% {1} {2}",
							this.investorID,
							this.systemBalance.NewBalance,
							this.systemBalance.NewBalance.Value / this.investorAccountingConfiguration.MonthlyFundingCapital.Value);
					SendFundsUtilized();
				} else if (this.investorAccountingConfiguration.MonthlyFundingCapital.HasValue &&
					this.investorAccountingConfiguration.MonthlyFundingCapital.Value > 0 &&
					this.systemBalance.NewBalance.Value / this.investorAccountingConfiguration.MonthlyFundingCapital.Value < CurrentValues.Instance.InvestorFundsUtilized90) {
						Log.Info("NotifyInvestorUtilizedFunds investor {0} balance less then 90% {1} {2}",
							this.investorID,
							this.systemBalance.NewBalance,
							this.systemBalance.NewBalance.Value / this.investorAccountingConfiguration.MonthlyFundingCapital.Value);
					SendFundsUtilized();
				} else if (this.investorAccountingConfiguration.FundingLimitForNotification.HasValue &&
					this.systemBalance.NewBalance.Value < this.investorAccountingConfiguration.FundingLimitForNotification.Value) {
					Log.Info("NotifyInvestorUtilizedFunds investor {0} balance less then FundingLimitForNotification {1} {2}",
						this.investorID,
						this.systemBalance.NewBalance,
						this.investorAccountingConfiguration.FundingLimitForNotification);
					SendFundsUtilized();
				} else {
					Log.Info("NotifyInvestorUtilizedFunds investor {0} balance is more than all the rules, not sending notification",
						this.investorID);
				} //if

			} catch (Exception ex) {
				Log.Error(ex, "failed to send funds utilized notification to investor {0}", this.investorID);
			}//try
		}//Execute

		private void SendFundsUtilized() {
			if (!this.systemBalance.NewBalance.HasValue) {
				return;
			}//if

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
				Log.Info("NotifyInvestorUtilizedFunds sending funds utilized notification to investor {0} contact {1} {2}", this.investorID, investorContact.InvestorContactID, investorContact.Email);
				mailer.Send("InvestorFundsNotification",
					parameters,
					new Addressee(
						investorContact.Email,
						bShouldRegister: true,
						userID: investorContact.InvestorContactID,
						addSalesforceActivity: false)
				);

			}//foreach
		}//SendFundsUtilized

		private readonly int investorID;
		private I_Investor investor;
		private I_InvestorAccountingConfiguration investorAccountingConfiguration;
		private I_InvestorBankAccount fundingBankAccount;
		private I_InvestorSystemBalance systemBalance;
	}//NotifyInvestorUtilizedFunds
}//ns
