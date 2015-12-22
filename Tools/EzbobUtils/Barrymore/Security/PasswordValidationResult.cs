namespace Ezbob.Utils.Security {
	/// <summary>
	/// Holds result of password validation.
	/// </summary>
	public class PasswordValidationResult {
		/// <summary>
		/// Casts <see cref="PasswordValidationResult"/> to boolean.
		/// </summary>
		/// <param name="v">Value to cast.</param>
		/// <returns>False, if <see cref="v"/> is null; <see cref="Match"/>, otherwise.</returns>
		public static implicit operator bool(PasswordValidationResult v) {
			return (v != null) && v.Match;
		} // operator bool

		/// <summary>
		/// Creates result instance.
		/// </summary>
		/// <param name="match">True, if there was match in passwords; false, otherwise.</param>
		public PasswordValidationResult(bool match) {
			Match = match;
			NewPassword = null;
		} // constructor

		/// <summary>
		/// True, if there was match in passwords; false, otherwise.
		/// </summary>
		public bool Match { get; private set; }

		/// <summary>
		/// Gets new serialized value of the password if password matches and
		/// number of iterations to generate currently stored value is less than
		/// current configured minimum number of iterations.
		/// <para>If this value is not null, it is expected to be serialized (e.g. stored to DB) instead of currently
		/// existing value.</para>
		/// </summary>
		public HashedPassword NewPassword { get; internal set; }
	} // class PasswordValidationResult
} // namespace
