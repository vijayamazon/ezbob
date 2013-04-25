namespace Integration.ChannelGrabberAPI {
	#region interface IJsonable

	public interface IJsonable {
		/// <summary>
		/// Converts current object into such object that all its public properties
		/// are exported into JSON format.
		/// </summary>
		/// <returns>Object to convert into JSON format.</returns>
		object ToJson();
	} // interface IJsonable

	#endregion interface IJsonable
} // namespace Integration.ChannelGrabberAPI
