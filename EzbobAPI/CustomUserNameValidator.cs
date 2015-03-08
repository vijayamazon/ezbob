namespace EzbobAPI {
	using System.Diagnostics;
	using System.IdentityModel.Selectors;

	public class CustomUserNameValidator : UserNamePasswordValidator {
		public override void Validate(string userName, string password) {
			Debug.WriteLine("userName {0}, password: {1}", userName, password);

		}
	}
}