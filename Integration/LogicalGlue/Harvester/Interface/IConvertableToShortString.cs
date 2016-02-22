namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	public interface IConvertableToShortString {
		/// <summary>
		/// Converts object to string representation. This representation can be equal to <see cref="ToString" />
		/// but is intended to be shorter (in order to write it to log and be no more than one-two lines).
		/// </summary>
		/// <returns>String representation of this object.</returns>
		string ToShortString();
	} // interface IConvertableToShortString
} // namespace
