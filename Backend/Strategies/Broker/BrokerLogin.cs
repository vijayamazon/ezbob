namespace Ezbob.Backend.Strategies.Broker {
	using System;
	using ConfigManager;
	using Exceptions;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;

	public class BrokerLogin : AStrategy {
		public BrokerLogin(
			string sEmail,
			DasKennwort sPassword,
			string promotionName,
			DateTime? promotionPageVisitTime,
			int uiOriginID
		) {
			this.spLoadDataForLoginCheck = new BrokerLoadLoginData(DB, Log) {
				Email = sEmail,
				UiOriginID = uiOriginID,
			};

			this.spOnSuccess = new BrokerLoginSucceeded(DB, Log) {
				LotteryCode = promotionName,
				PageVisitTime = promotionPageVisitTime,
			};

			this.password = sPassword.Decrypt();

			Properties = new BrokerProperties();
		} // constructor

		public override string Name { get { return "Broker login"; } } // Name

		public BrokerProperties Properties { get; private set; }

		public override void Execute() {
			ValidateCredentials();

			this.spOnSuccess.FillFirst(Properties);

			if (!string.IsNullOrWhiteSpace(Properties.ErrorMsg))
				throw new StrategyWarning(this, Properties.ErrorMsg);

			SafeReader sr = DB.GetFirst(
				"LoadActiveLotteries",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UserID", Properties.BrokerID),
				new QueryParameter("Now", DateTime.UtcNow)
			);

			Properties.LotteryPlayerID = sr.IsEmpty ? string.Empty : ((Guid)sr["UniqueID"]).ToString("N");
			Properties.LotteryCode = sr["LotteryCode"];
		} // Execute

		private void ValidateCredentials() {
			this.spOnSuccess.BrokerID = 0;

			SafeReader sr = this.spLoadDataForLoginCheck.GetFirst();

			if (sr.IsEmpty) {
				Log.Warn(
					"Could not find broker details by email '{0}' and origin '{1}'.",
					this.spLoadDataForLoginCheck.Email,
					this.spLoadDataForLoginCheck.UiOriginID
				);

				throw new StrategyWarning(this, "Invalid user name or password.");
			} // if

			string userName = sr["UserName"];
			int originID = sr["OriginID"];

			bool mismatch =
				!Normalize(this.spLoadDataForLoginCheck.Email).Equals(Normalize(userName)) ||
				(originID != this.spLoadDataForLoginCheck.UiOriginID);

			if (mismatch) {
				Log.Warn(
					"User name and origin returned by email '{0}' with origin '{1}' are '{2}', '{3}' " +
					"and differ from the requested user name and origin.",
					this.spLoadDataForLoginCheck.Email,
					this.spLoadDataForLoginCheck.UiOriginID,
					userName,
					originID
				);

				throw new StrategyWarning(this, "Invalid user name or password.");
			} // if

			var storedPassword = new HashedPassword(
				userName,
				(string)sr["CycleCount"],
				(string)sr["EzPassword"],
				(string)sr["Salt"]
			);

			var pu = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);

			PasswordValidationResult validationResult = pu.Validate(this.password, storedPassword);

			if (!validationResult.Match) {
				Log.Warn(
					"Invalid password specified for broker email '{0}' and origin '{1}'.",
					this.spLoadDataForLoginCheck.Email,
					this.spLoadDataForLoginCheck.UiOriginID
				);

				throw new StrategyWarning(this, "Invalid user name or password.");
			} // if

			this.spOnSuccess.BrokerID = sr["BrokerID"];

			if (validationResult.NewPassword == null) {
				this.spOnSuccess.EzPassword = null;
				this.spOnSuccess.Salt = null;
				this.spOnSuccess.CycleCount = null;
			} else {
				this.spOnSuccess.EzPassword = validationResult.NewPassword.Password;
				this.spOnSuccess.Salt = validationResult.NewPassword.Salt;
				this.spOnSuccess.CycleCount = validationResult.NewPassword.CycleCount;
			} // if

			Log.Msg(
				"Broker email '{0}' and origin '{1}' was validated as broker #{2}.",
				this.spLoadDataForLoginCheck.Email,
				this.spLoadDataForLoginCheck.UiOriginID,
				this.spOnSuccess.BrokerID
			);
		} // ValidateCredentials

		private readonly BrokerLoginSucceeded spOnSuccess;
		private readonly BrokerLoadLoginData spLoadDataForLoginCheck;
		private readonly string password;

		private static string Normalize(string s) {
			return (s ?? string.Empty).Trim().ToLowerInvariant();
		} // Normalize

		private class BrokerLoadLoginData : AStoredProcedure {
			public BrokerLoadLoginData(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor
			
			[UsedImplicitly]
			public string Email { get; set; }

			public override bool HasValidParameters() {
				Email = MiscUtils.ValidateStringArg(Email, "Email");

				return !string.IsNullOrWhiteSpace(Email) && (UiOriginID > 0);
			} // HasValidParameters

			[UsedImplicitly]
			public int UiOriginID { get; set; }
		} // class BrokerLoadLoginData

		private class BrokerLoginSucceeded : AStoredProcedure {
			public BrokerLoginSucceeded(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			public override bool HasValidParameters() {
				bool hasPassword =
					!string.IsNullOrWhiteSpace(EzPassword) &&
					!string.IsNullOrWhiteSpace(Salt) &&
					!string.IsNullOrWhiteSpace(CycleCount);

				bool hasNoPassword =
					(EzPassword == null) &&
					(Salt == null) &&
					(CycleCount == null);

				bool passwordOk = hasNoPassword || hasPassword;

				return passwordOk && (BrokerID > 0);
			} // HasValidParameters

			[UsedImplicitly]
			public int BrokerID { get; set; }

			[UsedImplicitly]
			public string EzPassword { get; set; }

			[UsedImplicitly]
			public string Salt { get; set; }

			[UsedImplicitly]
			public string CycleCount { get; set; }

			[UsedImplicitly]
			public string LotteryCode { get; set; }

			[UsedImplicitly]
			public DateTime? PageVisitTime { get; set; }
		} // class BrokerLoginSucceeded
	} // class BrokerLogin
} // namespace Ezbob.Backend.Strategies.Broker
