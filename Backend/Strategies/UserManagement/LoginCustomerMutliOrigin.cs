namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using System.Data;
	using System.Diagnostics.CodeAnalysis;
	using System.Web.Security ;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.dbutils;
	using JetBrains.Annotations;

	public class LoginCustomerMutliOrigin : AStrategy {
		public LoginCustomerMutliOrigin(LoginCustomerMultiOriginModel model) {
			this.model = model;
			this.spLoad = new CustomerLoadLoginData(this);
		} // constructor

		public override string Name {
			get { return "LoginCustomerMutliOrigin"; }
		} // Name

		public override void Execute() {
			this.spLoad.Execute();

			string originName = this.model.Origin.HasValue ? this.model.Origin.Value.ToString() : "-- null --";

			if (this.spLoad.Result.UserID < 1) {
				Log.Warn(
					"Customer log on attempt from remote IP {0} with user name '{1}' and origin '{2}': " +
					"could not find a user entry.",
					this.model.RemoteIp,
					this.model.UserName,
					originName
				);

				SetOutputValues(false, 0, 0, null, MembershipCreateStatus.InvalidUserName);
				return;
			} // if user not found

			Log.Debug("Customer log on attempt with login '{0}' and origin '{1}'.", this.model.UserName, originName);

			Log.Debug(
				"Customer log on attempt from remote IP {0} with user name '{1}' and origin '{2}': log on attempt #{3}.",
				this.model.RemoteIp,
				this.model.UserName,
				originName,
				this.spLoad.Result.LoginFailedCount + 1
			);

			if (this.spLoad.Result.IsDisabled) {
				string sDisabledError =
					"This account is closed, please contact customer care<br/> " +
					this.spLoad.Result.CustomerCareEmail;

				var sp = new CreateCustomerSession(DB, Log) {
					CustomerID = this.spLoad.Result.UserID,
					Ip = this.model.RemoteIp,
					ErrorMessage = sDisabledError,
				};

				int sessionID = sp.ExecuteScalar<int>();

				Log.Warn(
					"Customer log on attempt from remote IP {0} with user name '{1}' and origin '{2}': " +
					"the customer is disabled.",
					this.model.RemoteIp,
					this.model.UserName,
					this.model.Origin
				);

				SetOutputValues(
					false,
					this.spLoad.Result.UserID,
					sessionID,
					this.spLoad.Result.RefNumber,
					MembershipCreateStatus.UserRejected,
					sDisabledError
				);

				return;
			} // if user is disabled

			var loginStra = new UserLogin(
				this.model.Origin,
				this.model.UserName,
				this.model.Password,
				this.model.RemoteIp,
				this.model.PromotionName,
				this.model.PromotionPageVisitTime
			);

			loginStra.Execute();

			if (MembershipCreateStatus.Success == loginStra.Status) {
				Log.Debug(
					"Customer log on attempt from remote IP {0} with user name '{1}' and origin '{2}': success.",
					this.model.RemoteIp,
					this.model.UserName,
					this.model.Origin
				);

				SetOutputValues(
					true,
					this.spLoad.Result.UserID,
					loginStra.SessionID,
					this.spLoad.Result.RefNumber,
					loginStra.Status,
					null
				);

				return;
			} // if logged in successfully

			string errorMessage = MembershipCreateStatus.InvalidProviderUserKey == loginStra.Status
				? string.Format(
					"Three unsuccessful login attempts have been made. " +
					"<span class='bold'>{0}" + "</span> has issued you with a temporary password. " +
					"Please check your e-mail.",
					this.spLoad.Result.OriginName
				)
				: StandardErrror;

			Log.Warn(
				"Customer log on attempt from remote IP {0} with user name '{1}' and origin '{2}' failed: {3}.",
				this.model.RemoteIp,
				this.model.UserName,
				this.model.Origin,
				errorMessage
			);

			SetOutputValues(
				false,
				this.spLoad.Result.UserID,
				loginStra.SessionID,
				this.spLoad.Result.RefNumber,
				loginStra.Status,
				errorMessage
			);
		} // Execute

		public bool Success { get; private set; }
		public int UserID { get; private set; }
		public int SessionID { get; private set; }
		public string ErrorMsg { get; private set; }
		public string RefNumber { get; private set; }
		public MembershipCreateStatus Status { get; private set; }

		private void SetOutputValues(
			bool success,
			int userID,
			int sessionID,
			string refNumber,
			MembershipCreateStatus status,
			string errorMsg = StandardErrror
		) {
			Success = success;
			UserID = userID;
			SessionID = sessionID;
			ErrorMsg = errorMsg == StandardErrror ? "User not found or incorrect password." : errorMsg;
			RefNumber = refNumber;
			Status = status;
		} // SetOutputValues

		private const string StandardErrror = "STD_ERR";

		private readonly CustomerLoadLoginData spLoad;

		[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		private class CustomerLoadLoginData : AStoredProcedure {
			public CustomerLoadLoginData(LoginCustomerMutliOrigin stra) : base(stra.DB, stra.Log) {
				this.stra = stra;
			} // constructor

			public override bool HasValidParameters() {
				return !string.IsNullOrWhiteSpace(UserName) && (OriginID > 0);
			} // HasValidParameters

			[Length(250)]
			public string UserName {
				get { return this.stra.model.UserName; }
				set { }
			} // UserName

			public int OriginID {
				get { return this.stra.model.Origin.HasValue ? (int)this.stra.model.Origin : 0; }
				set { }
			} // OriginID

			public LoginData Result { get; private set; }

			public void Execute() {
				Result = FillFirst<LoginData>();
			} // Execute

			[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
			public class LoginData {
				public int UserID { get; set; }
				public bool IsDisabled { get; set; }
				public string CustomerCareEmail { get; set; }
				public string RefNumber { get; set; }
				public int LoginFailedCount { get; set; }
				public string OriginName { get; set; }
			} // class LoginData

			private readonly LoginCustomerMutliOrigin stra;
		} // class CustomerLoadLoginData

		[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		private class CreateCustomerSession : AStoredProcedure {
			public CreateCustomerSession(AConnection db, ASafeLog log) : base(db, log) {}

			public override bool HasValidParameters() {
				return CustomerID > 0;
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerID { get; set; }

			[UsedImplicitly]
			public DateTime StartSession {
				get { return DateTime.UtcNow; }
				set { }
			} // StartSession

			[UsedImplicitly]
			public DateTime EndSession {
				get { return DateTime.UtcNow; }
				set { }
			} // EndSession

			[UsedImplicitly]
			public string Ip { get; set; }

			[UsedImplicitly]
			public bool IsPasswdOk {
				get { return false; }
				set { }
			} // IsPasswordOk

			[UsedImplicitly]
			public string ErrorMessage { get; set; }

			[UsedImplicitly]
			[Direction(ParameterDirection.Output)]
			public int SessionID { get; set; }
		} // CreateCustomerSession

		private readonly LoginCustomerMultiOriginModel model;
	} // class LoginCustomerMutliOrigin
} // namespace

