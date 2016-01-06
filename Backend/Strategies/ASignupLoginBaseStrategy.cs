namespace Ezbob.Backend.Strategies {
	public abstract class ASignupLoginBaseStrategy : AStrategy {
		protected static string NormalizeUserName(string userName) {
			return (userName ?? string.Empty).Trim().ToLowerInvariant();
		} // NormalizeUserName
	} // class ASignupLoginBaseStrategy
} // namespace

