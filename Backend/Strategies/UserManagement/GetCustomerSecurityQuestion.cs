namespace Ezbob.Backend.Strategies.UserManagement {
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.dbutils;
	using EZBob.DatabaseLib.Model.Database;

	public class GetCustomerSecurityQuestion : AStrategy {
		public GetCustomerSecurityQuestion(string email, CustomerOriginEnum origin) {
			this.sp = new SpGetCustomerSecurityQuestion(DB, Log) {
				Email = email,
				OriginID = (int)origin
			};
		} // constructor

		public override string Name {
			get { return "GetCustomerSecurityQuestion"; }
		} // Name

		public override void Execute() {
			this.sp.ExecuteNonQuery();

			Log.Debug(
				"GetCustomerSecurityQuestion({0}, {1}) = (result = {2}) '{3}'",
				this.sp.Email,
				this.sp.OriginID,
				this.sp.Result,
				this.sp.SecurityQuestion
			);
		} // Execute

		public string SecurityQuestion { get { return this.sp.SecurityQuestion; } }

		private readonly SpGetCustomerSecurityQuestion sp;

		private class SpGetCustomerSecurityQuestion : AStoredProc {
			public SpGetCustomerSecurityQuestion(AConnection db, ASafeLog log) : base(db, log) {} // constructor

			public override bool HasValidParameters() {
				return !string.IsNullOrWhiteSpace(Email) && (OriginID > 0);
			} // HasValidParameters

			[Length(250)]
			public string Email { get; set; }

			public int OriginID { get; set; }

			[Direction(ParameterDirection.ReturnValue)]
			public int Result { get; set; }

			[Length(200)]
			[Direction(ParameterDirection.Output)]
			public string SecurityQuestion { get; set; }
		} // class SpGetCustomerSecurityQuestion
	} // class GetCustomerSecurityQuestion
} // namespace

