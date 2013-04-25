namespace Integration.ChannelGrabberAPI {
	#region class Jsonable

	public abstract class AJsonable : IJsonable {
		#region public

		/// <summary>
		/// Default implementation of conversion to JSON.
		/// </summary>
		/// <returns>Reference to this object.</returns>
		public virtual object ToJson() {
			return this;
		} // ToJson

		#endregion public
	} // Jsonable

	#endregion class Jsonable
} // namespace Integration.ChannelGrabberAPI
