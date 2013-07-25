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
    public class AvailableFundsValidator
    {
        private readonly PacNetBalanceRepository _funds;
        private readonly PacNetManualBalanceRepository _manualFunds;
        private readonly IEzBobConfiguration _config;
        private static readonly ILog Log = LogManager.GetLogger(typeof(LoanCreator));

        public AvailableFundsValidator(PacNetBalanceRepository funds, PacNetManualBalanceRepository manualFunds, IEzBobConfiguration config)
        {
            _funds = funds;
            _manualFunds = manualFunds;
            _config = config;
        }

        public virtual void VerifyAvailableFunds(decimal transfered)
        {
            try
            {
                var balance = _funds.GetBalance();
                var manualBalance = _manualFunds.GetBalance();
                var fundsAvailable = balance.Adjusted + manualBalance - transfered;

                DateTime today = DateTime.UtcNow;
                int relevantLimit = (today.DayOfWeek == DayOfWeek.Thursday || today.DayOfWeek == DayOfWeek.Friday) ? _config.PacnetBalanceWeekendLimit : _config.PacnetBalanceWeekdayLimit;
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

            var result = mail.Send(vars, _config.NotEnoughFundsToAddess, _config.NotEnoughFundsTemplateName);
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