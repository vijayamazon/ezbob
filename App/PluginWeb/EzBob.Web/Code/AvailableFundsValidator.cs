using System;
using System.Collections.Generic;
using System.Globalization;
using EZBob.DatabaseLib;
using EzBob.Web.Infrastructure;
using log4net;
using MailApi;
using StructureMap;


namespace EzBob.Web.Code
{
	using ConfigManager;

	public class AvailableFundsValidator
    {
        private readonly PacNetBalanceRepository _funds;
        private readonly PacNetManualBalanceRepository _manualFunds;
		private static readonly ILog Log = LogManager.GetLogger(typeof(AvailableFundsValidator));

        public AvailableFundsValidator(PacNetBalanceRepository funds, PacNetManualBalanceRepository manualFunds)
        {
            _funds = funds;
            _manualFunds = manualFunds;
        }

        public virtual void VerifyAvailableFunds(decimal transfered)
        {
            try
            {
                var balance = _funds.GetBalance();
                var manualBalance = _manualFunds.GetBalance();
                var fundsAvailable = balance.Adjusted + manualBalance - transfered;
				
                var today = DateTime.UtcNow;
				int relevantLimit = (today.DayOfWeek == DayOfWeek.Thursday || today.DayOfWeek == DayOfWeek.Friday) ? CurrentValues.Instance.PacnetBalanceWeekendLimit : CurrentValues.Instance.PacnetBalanceWeekdayLimit;
				Log.InfoFormat("VerifyAvailableFunds pacnet balance {0} manual balance {1} transfered {2} funds available {3} relevant limit {4}", balance, manualBalance, transfered, fundsAvailable, relevantLimit);
                if (fundsAvailable < relevantLimit)
                {
                    SendMail(fundsAvailable, relevantLimit);
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Failed verifying available funds with error:{0}", e);
            }
        }

        private void SendMail(decimal currentFunds, int requiredFunds)
        {
            var mail = ObjectFactory.GetInstance<IMail>();
            var vars = new Dictionary<string, string>
				{
					{"CurrentFunds", currentFunds.ToString("N2", CultureInfo.InvariantCulture)},
					{"RequiredFunds", requiredFunds.ToString("N", CultureInfo.InvariantCulture)} 
				};

			var result = mail.Send(vars, CurrentValues.Instance.NotEnoughFundsToAddress, CurrentValues.Instance.NotEnoughFundsTemplateName);
            if (result == "OK")
            {
                Log.InfoFormat("Sent mail - not enough funds");
            }
            else
            {
                Log.ErrorFormat("Failed sending alert mail - not enough funds. Result:{0}", result);
            }
        }
    }
}