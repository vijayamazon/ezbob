namespace Ezbob.Backend.Strategies.Broker {
    using System;
    using System.Text.RegularExpressions;
    using ConfigManager;
    using Ezbob.Backend.Models;
    using Ezbob.Backend.Strategies.Exceptions;
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
            try
            {
                if (string.IsNullOrEmpty(this.model.AccountNumber) || !Regex.IsMatch(this.model.AccountNumber, @"^\d{8}$"))
                {
                    throw new StrategyWarning(this, "Invalid account number");
                }

                if (string.IsNullOrEmpty(this.model.SortCode) || !Regex.IsMatch(this.model.SortCode, @"^\d{6}$"))
                {
                    throw new StrategyWarning(this, "Invalid sort code");
                }

                ISortCodeChecker sortCodeChecker;
                if (CurrentValues.Instance.PostcodeAnywhereEnabled)
                {
                    sortCodeChecker = new SortCodeChecker(CurrentValues.Instance.PostcodeAnywhereMaxBankAccountValidationAttempts);
                }
                else
                {
                    sortCodeChecker = new FakeSortCodeChecker();
                }

                var brokerRepository = ObjectFactory.GetInstance<BrokerRepository>();
                var broker = brokerRepository.Find(this.model.BrokerEmail);

                if (broker == null) {
                    Log.Alert("BrokerAddBank broker not found by email {0}", this.model.BrokerEmail);
                    throw new StrategyWarning(this, "Failed adding bank account");
                }

                var cardInfo = new CardInfo
                {
                    BankAccount = this.model.AccountNumber,
                    SortCode = this.model.SortCode,
                    Type = (BankAccountType)Enum.Parse(typeof(BankAccountType), this.model.BankAccountType),
                    Broker = broker,
                    IsDefault = true
                };

                sortCodeChecker.Check(cardInfo);

                foreach (var bankAccount in broker.BankAccounts) {
                    bankAccount.IsDefault = false;
                }

                broker.BankAccounts.Add(cardInfo);
                brokerRepository.SaveOrUpdate(broker);
            }
            catch (SortCodeNotFoundException)
            {
                throw new StrategyWarning(this, "Sort code was not found");
            }
            catch (UnknownSortCodeException)
            {
                throw new StrategyWarning(this, "Sort code was not found");
            }
            catch (InvalidAccountNumberException)
            {
                throw new StrategyWarning(this, "Account number is not valid");
            }
            catch (NotValidSortCodeException)
            {
                throw new StrategyWarning(this, "Sort code is not valid");
            }
		} // Execute

        private readonly BrokerAddBankModel model;
	} // class BrokerAddBank
} // namespace Ezbob.Backend.Strategies.Broker
