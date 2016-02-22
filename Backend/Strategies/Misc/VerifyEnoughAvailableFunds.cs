namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Web;
	using ConfigManager;
	using MailApi;

	public class VerifyEnoughAvailableFunds : AStrategy {
		private readonly decimal deductAmount;

		public VerifyEnoughAvailableFunds(decimal deductAmount) {
			this.deductAmount = deductAmount;
			HasEnoughFunds = true;
		} // constructor

		public override string Name {
			get { return "Verify Enough Available Funds"; }
		} // Name

		public bool HasEnoughFunds { get; private set; }

		public override void Execute() {
			var getAvailableFunds = new GetAvailableFunds();
			getAvailableFunds.Execute();
			decimal availableFunds = getAvailableFunds.AvailableFunds;

			var today = DateTime.UtcNow;
			int relevantLimit = (today.DayOfWeek == DayOfWeek.Thursday || today.DayOfWeek == DayOfWeek.Friday) ? CurrentValues.Instance.PacnetBalanceWeekendLimit : CurrentValues.Instance.PacnetBalanceWeekdayLimit;

			Log.Info("AvailableFunds:{0} Required:{1} Deducted:{2}", availableFunds, relevantLimit, deductAmount);
			if (availableFunds - deductAmount < relevantLimit) {
				HasEnoughFunds = false;
				SendMail(availableFunds - deductAmount, relevantLimit);
			}
		} // Execute

		private void SendMail(decimal currentFunds, int requiredFunds) {
			var mail = new Mail();
			var vars = new Dictionary<string, string>
				{
					{"CurrentFunds", currentFunds.ToString("N2", CultureInfo.InvariantCulture)},
					{"RequiredFunds", requiredFunds.ToString("N", CultureInfo.InvariantCulture)} ,
					{"MachineName", System.Environment.MachineName},
					{"ServerName", HttpContext.Current != null && HttpContext.Current.Server != null ? HttpContext.Current.Server.MachineName : "" },
					{"HostName", System.Net.Dns.GetHostName() },
				};

			var result = mail.Send(vars, CurrentValues.Instance.NotEnoughFundsToAddress, CurrentValues.Instance.NotEnoughFundsTemplateName);
			if (result == "OK") {
				Log.Info("Sent mail - not enough funds");
			}
			else {
				Log.Error("Failed sending alert mail - not enough funds. Result:{0}", result);
			}
		}
	} // class DisableCurrentManualPacnetDeposits
} // namespace