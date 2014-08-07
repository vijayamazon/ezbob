namespace EzBob.Backend.Strategies.Misc
{
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;

	[DataContract]
	public enum PasswordResetTarget
	{
		[EnumMember]
		Customer,

		[EnumMember]
		Broker,
	}

	public class ResetPassword123456 : AStrategy
	{
		private readonly LoadEmailForPasswordReset spLoad;

		public ResetPassword123456(
			int targetId,
			PasswordResetTarget target,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) 
		{
			spLoad = new LoadEmailForPasswordReset(DB, Log) 
			{
				TargetID = targetId,
				Target = target.ToString(),
			};
		}

		public override string Name { get { return "ResetPassword123456"; } }

		public override void Execute()
		{
			Log.Debug("Resetting password for {0} {1}...", spLoad.Target, spLoad.TargetID);

			string sEmail = spLoad.ExecuteScalar<string>();

			if (string.IsNullOrWhiteSpace(sEmail)) 
			{
				Log.Warn("Resetting password for {0} {1} failed: no email found.", spLoad.Target, spLoad.TargetID);
				return;
			}

			var sp = new SavePassword(spLoad, DB, Log) {
				Password = SecurityUtils.HashPassword(sEmail, "123456"),
			};

			sp.ExecuteNonQuery();

			Log.Debug("Resetting password for {0} {1} complete.", spLoad.Target, spLoad.TargetID);
		}


		private class LoadEmailForPasswordReset : AStoredProcedure
		{
			public LoadEmailForPasswordReset(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {}

			public override bool HasValidParameters()
			{
				if (TargetID <= 0)
					return false;

				PasswordResetTarget t;

				return Enum.TryParse<PasswordResetTarget>(Target, out t);
			}

			[UsedImplicitly]
			public int TargetID { get; set; }

			[UsedImplicitly]
			public string Target { get; set; }
		}

		private class SavePassword : AStoredProcedure
		{
			public SavePassword(LoadEmailForPasswordReset oSpLoad, AConnection oDB, ASafeLog oLog) : base(oDB, oLog)
			{
				TargetID = oSpLoad.TargetID;
				Target = oSpLoad.Target;
			}

			public override bool HasValidParameters()
			{
				if ((TargetID <= 0) || string.IsNullOrWhiteSpace(Password))
					return false;

				PasswordResetTarget t;

				return Enum.TryParse<PasswordResetTarget>(Target, out t);
			}

			[UsedImplicitly]
			public int TargetID { get; set; }

			[UsedImplicitly]
			public string Target { get; set; }

			[UsedImplicitly]
			public string Password { get; set; }
		}
	}
}
