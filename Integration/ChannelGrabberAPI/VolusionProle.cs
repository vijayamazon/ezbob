using EZBob.DatabaseLib.Model.Database;
using log4net;

namespace Integration.ChannelGrabberAPI {
	#region class VolusionProle

	public class VolusionProle : Prole {
		public VolusionProle(ILog log, Customer customer) : base(log, customer) {}

		public override ShopTypes ShopType { get { return ShopTypes.Volusion; } }
	} // class VolusionProle

	#endregion class VolusionProle
} // namespace Integration.ChannelGrabberAPI
