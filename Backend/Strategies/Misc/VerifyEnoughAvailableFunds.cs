namespace EzBob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailApi;

	public class VerifyEnoughAvailableFunds : AStrategy
	{
		private readonly decimal deductAmount;
		#region constructor

		public VerifyEnoughAvailableFunds(decimal deductAmount, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.deductAmount = deductAmount;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Verify Enough Available Funds"; }
		} // Name

		#endregion property Name

		public bool HasEnoughFunds { get; private set; }
		
		#region property Execute

		public override void Execute() {
			var getAvailableFunds = new GetAvailableFunds(DB, Log);
			getAvailableFunds.Execute();
			decimal availableFunds = getAvailableFunds.AvailableFunds;

			var today = DateTime.UtcNow;
			int relevantLimit = (today.DayOfWeek == DayOfWeek.Thursday || today.DayOfWeek == DayOfWeek.Friday) ? CurrentValues.Instance.PacnetBalanceWeekendLimit : CurrentValues.Instance.PacnetBalanceWeekdayLimit;

			Log.Info("AvailableFunds:{0} Required:{1} Deducted:{2}", availableFunds, relevantLimit, deductAmount);
			if (availableFunds - deductAmount < relevantLimit)
			{
				SendMail(availableFunds - deductAmount, relevantLimit);
			}
		} // Execute

		#endregion property Execute

		private void SendMail(decimal currentFunds, int requiredFunds)
		{
			var mail = new Mail();
			var vars = new Dictionary<string, string>
				{
					{"CurrentFunds", currentFunds.ToString("N2", CultureInfo.InvariantCulture)},
					{"RequiredFunds", requiredFunds.ToString("N", CultureInfo.InvariantCulture)} 
				};

			var result = mail.Send(vars, CurrentValues.Instance.NotEnoughFundsToAddress, CurrentValues.Instance.NotEnoughFundsTemplateName);
			if (result == "OK")
			{
				Log.Info("Sent mail - not enough funds");
			}
			else
			{
				Log.Error("Failed sending alert mail - not enough funds. Result:{0}", result);
			}
		}
	} // class DisableCurrentManualPacnetDeposits
} // namespace
