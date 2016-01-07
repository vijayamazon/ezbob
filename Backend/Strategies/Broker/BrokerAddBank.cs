namespace Ezbob.Backend.Strategies.Broker {
	using System;
	using System.Data;
	using System.Linq;
	using System.Text.RegularExpressions;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Broker;
	using PostcodeAnywhere;
	using StructureMap;

	public class BrokerAddBank : AStrategy {
		public BrokerAddBank(BrokerAddBankModel model) {
			this.model = model;
		} // constructor

		public override string Name {
			get { return ""; }
		} // Name

		public override void Execute() {
			Broker broker = null;
			try {
				if (string.IsNullOrEmpty(this.model.AccountNumber) || !Regex.IsMatch(this.model.AccountNumber, @"^\d{8}$"))
					throw new StrategyWarning(this, "Invalid account number");

				if (string.IsNullOrEmpty(this.model.SortCode) || !Regex.IsMatch(this.model.SortCode, @"^\d{6}$"))
					throw new StrategyWarning(this, "Invalid sort code");

				ISortCodeChecker sortCodeChecker = CurrentValues.Instance.PostcodeAnywhereEnabled
					? (ISortCodeChecker)new SortCodeChecker(CurrentValues.Instance.PostcodeAnywhereMaxBankAccountValidationAttempts)
					: (ISortCodeChecker)new FakeSortCodeChecker(CurrentValues.Instance.PostcodeAnywhereMaxBankAccountValidationAttempts);

				var sp = new FindBrokerID(this.model, DB, Log);
				sp.ExecuteNonQuery();

				if (sp.BrokerID <= 0) {
					Log.Alert("BrokerAddBank broker id not found by email {0}", this.model.BrokerEmail);
					throw new StrategyWarning(this, "Failed adding bank account");
				} // if

				var brokerRepository = ObjectFactory.GetInstance<BrokerRepository>();
				broker = brokerRepository.GetByID(sp.BrokerID);

				if (broker == null) {
					Log.Alert("BrokerAddBank broker not found by id {0}", sp.BrokerID);
					throw new StrategyWarning(this, "Failed adding bank account");
				} // if

				var cardInfo = new CardInfo {
					BankAccount = this.model.AccountNumber,
					SortCode = this.model.SortCode,
					Type = (BankAccountType)Enum.Parse(typeof(BankAccountType), this.model.BankAccountType),
					Broker = broker,
					IsDefault = true
				};

				sortCodeChecker.Check(cardInfo);

				foreach (CardInfo bankAccount in broker.BankAccounts)
					bankAccount.IsDefault = false;

				broker.BankAccounts.Add(cardInfo);

				var loanCommissionsWithoutCard = broker.LoanBrokerCommissions.Where(x =>
					x.CardInfo == null && x.PaidDate == null && x.Status != "Done"
				);

				foreach (var commission in loanCommissionsWithoutCard)
					commission.CardInfo = cardInfo;

				brokerRepository.SaveOrUpdate(broker);
			} catch (SortCodeNotFoundException) {
				throw new StrategyWarning(this, "Sort code was not found");
			} catch (UnknownSortCodeException) {
				throw new StrategyWarning(this, "Sort code was not found");
			} catch (InvalidAccountNumberException) {
				throw new StrategyWarning(this, "Account number is not valid");
			} catch (NotValidSortCodeException) {
				throw new StrategyWarning(this, "Sort code is not valid");
			} catch (Exception ex) {
				Log.Error(ex, "BrokerAddBank failed " + this.model.BrokerEmail + " id: " + (broker == null ? 0 : broker.ID));
				throw new StrategyWarning(this, "Failed adding bank account");
			} // try
		} // Execute

		private class FindBrokerID : AStoredProcedure {
			public FindBrokerID(BrokerAddBankModel model, AConnection db, ASafeLog log) : base(db, log) {
				ContactEmail = model.BrokerEmail;
				Origin = (int)model.Origin;
				BrokerID = 0;
			} // constructor

			public override bool HasValidParameters() {
				return !string.IsNullOrWhiteSpace(ContactEmail) && (Origin > 0);
			} // HasValidParameters

			public string ContactEmail { get; set; }

			public int Origin { get; set; }

			[Direction(ParameterDirection.Output)]
			public int BrokerID { get; set; }
		} // class FindBrokerID

		private readonly BrokerAddBankModel model;
	} // class BrokerAddBank
} // namespace Ezbob.Backend.Strategies.Broker
