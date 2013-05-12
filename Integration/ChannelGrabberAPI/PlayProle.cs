using EZBob.DatabaseLib.Model.Database;
using log4net;

namespace Integration.ChannelGrabberAPI {
	#region class PlayProle

	/// <summary>
	/// Play specific account data.
	/// </summary>
	public class PlayProle : Prole {
		#region public

		#region constructor

		public PlayProle(ILog log, Customer customer) : base(log, customer) {} // constructor

		#endregion constructor

		#region property ShopType

		public override ShopTypes ShopType { get { return ShopTypes.Play; } }

		#endregion property ShopType

		#endregion public
	} // class PlayProle

	#endregion class PlayProle
} // namespace Integration.ChannelGrabberAPI
