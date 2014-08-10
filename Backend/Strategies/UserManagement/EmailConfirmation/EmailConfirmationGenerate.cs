namespace EzBob.Backend.Strategies.UserManagement.EmailConfirmation 
{
	using System;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class EmailConfirmationGenerate : AStrategy 
	{
		public EmailConfirmationGenerate(int userId, AConnection db, ASafeLog log)
			: base(db, log) 
		{
			Address = string.Empty;

			sp = new SpEmailConfirmationGenerate(userId, DB, Log);
		}

		public override string Name { get { return "EmailConfirmationGenerate"; } }

		public override void Execute() 
		{
			sp.ExecuteNonQuery();
			m_oSp.ExecuteNonQuery();
			Address = string.Format("<a href='{0}/confirm/{1}'>click here</a>", CurrentValues.Instance.CustomerSite.Value, Token);

			Log.Debug("Confirmation token {0} has been created for user {1}.", Token.ToString("N"), sp.UserID);
		}

		public Guid Token { get { return sp.Token; } }

		public string Address { get; private set; }

		private readonly SpEmailConfirmationGenerate sp;

		// ReSharper disable ValueParameterNotUsed
		private class SpEmailConfirmationGenerate : AStoredProc 
		{
			public SpEmailConfirmationGenerate(int userId, AConnection db, ASafeLog log)
				: base(db, log) 
			{
				token = Guid.NewGuid();
				UserID = userId;
			}

			public override bool HasValidParameters() 
			{
				return UserID > 0;
			}

			#region property Token

			[UsedImplicitly]
			public Guid Token
			{
				get { return token; }
				set { }
			}

			private readonly Guid token;

			#endregion property Token

			[UsedImplicitly]
			public int UserID { get; set; }

			[UsedImplicitly]
			public int EmailStateID 
			{
				get { return (int)EmailConfirmationRequestState.Pending; }
				set { }
			}

			[UsedImplicitly]
			public DateTime Now 
			{
				get { return DateTime.UtcNow; }
				set { }
			}
		}
		// ReSharper restore ValueParameterNotUsed
	}
}
