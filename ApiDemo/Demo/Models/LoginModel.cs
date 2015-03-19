namespace Demo.Models {
	/// <summary>
	/// Data required for logging in.
	/// </summary>
	public class LoginModel {
		/// <summary>
		/// User name (of customer/underwriter/broker).
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// Password.
		/// </summary>
		public string Password { get; set; }
	} // class LoginModel
} // namespace
