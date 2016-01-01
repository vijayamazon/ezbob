namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using System.Data;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.dbutils;
	using EZBob.DatabaseLib.Model.Database;

	public class ValidateSecurityAnswer : AStrategy {
		public ValidateSecurityAnswer(string email, CustomerOriginEnum origin, string answer) {
			ErrorMsg = null;

			this.answer = answer;

			this.sp = new LoadSecurityAnswer(DB, Log) {
				Email = email,
				OriginID = (int)origin,
			};
		} // constructor

		public override string Name {
			get { return "ValidateSecurityAnswer"; }
		} // Name

		public override void Execute() {
			if (string.IsNullOrWhiteSpace(this.answer)) {
				ErrorMsg = "Answer is empty.";
				return;
			} // if

			this.sp.ExecuteNonQuery();

			if (this.sp.Result != 0) {
				Log.Warn(
					"User not found for email '{0}' with origin '{1}'.",
					this.sp.Email,
					this.sp.OriginID
				);

				ErrorMsg = "User account not found by email " + this.sp.Email;
				return;
			} // if

			if (string.IsNullOrWhiteSpace(this.answer)) {
				Log.Alert(
					"Empty security answer is stored for customer '{0}' with origin '{1}'.",
					this.sp.Email,
					this.sp.OriginID
				);

				ErrorMsg = "Cannot check the answer, please call customer support.";
				return;
			} // if

			if (!this.answer.Equals(this.sp.Answer, StringComparison.InvariantCulture)) {
				ErrorMsg = "Wrong answer to security question.";
				return;
			} // if

			FireToBackground(new PasswordRestored(this.sp.UserID));
		} // Execute

		public string ErrorMsg { get; private set; }

		private readonly string answer;
		private readonly LoadSecurityAnswer sp;

		private class LoadSecurityAnswer : AStoredProcedure {
			public LoadSecurityAnswer(AConnection db, ASafeLog log) : base(db, log) {} // constructor

			public override bool HasValidParameters() {
				return !string.IsNullOrWhiteSpace(Email) && (OriginID > 0);
			} // HasValidParameters

			[Length(250)]
			public string Email { get; set; }

			public int OriginID { get; set; }

			[Length(200)]
			[Direction(ParameterDirection.Output)]
			public string Answer { get; set; }

			[Direction(ParameterDirection.Output)]
			public int UserID { get; set; }

			[Direction(ParameterDirection.ReturnValue)]
			public int Result { get; set; }
		} // class LoadSecurityAnswer
	} // class ValidateSecurityAnswer
} // namespace

