namespace Integration.ChannelGrabberAPI {
	#region interface IJsonable

	public interface IJsonable {
		object ToJson();
	} // interface IJsonable

	#endregion interface IJsonable

	#region class Jsonable

	public abstract class AJsonable : IJsonable {
		public virtual object ToJson() {
			return this;
		} // ToJson

		protected AJsonable() {} // constructor
	} // Jsonable

	#endregion class Jsonable
} // namespace Integration.ChannelGrabberAPI
