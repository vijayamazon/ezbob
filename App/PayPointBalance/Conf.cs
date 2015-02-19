namespace PayPointBalance {
	using System;
	using System.Collections.Generic;
	using ConfigurationBase;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;

	public class Conf : Configurations {
		public Conf(ASafeLog oLog = null) : base("LoadPaypointAccountsForReconciliation", oLog) {
		} // constructor

		public override void Init() {
			Reload("Init");
		} // Init

		public override void Refresh() {
			Reload("Refresh");
		} // Refresh

		public List<Account> Accounts { get; private set; }

		public class Account {
			public virtual string Mid { get; set; }
			public virtual string VpnPassword { get; set; }
			public virtual string RemotePassword { get; set; }
			public virtual int RetryCount { get { return 5; } }
			public virtual int SleepInterval { get { return 30; } }

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>
			/// A string that represents the current object.
			/// </returns>
			public override string ToString() {
				return string.Format(
					"Mid: {0}, VPN password: ******, remote password: ++++++, retry {1} every {2}",
					Mid,
					Grammar.Number(RetryCount, "time"),
					Grammar.Number(SleepInterval, "second")
				);
			} // ToString
		} // class Account

		private void Reload(string actionName) {
			try {
				Info("{0} Paypoint accounts configuration started...", actionName);

				AConnection oDB = DbConnectionGenerator.Get(this);

				Accounts = oDB.Fill<Account>(SpName, CommandSpecies.StoredProcedure);

				Info(
					"{0} Paypoint accounts configuration complete, {1} loaded.",
					actionName,
					Grammar.Number(Accounts.Count, "account")
				);
			} catch (Exception e) {
				Alert(e, "Failed to reload Paypoint accounts configuration.");
			} // try
		} // Reload
	} // class Conf
} // namespace
