using EZBob.DatabaseLib.Model.Database;
using log4net;

namespace Integration.ChannelGrabberAPI {
	#region class VolusionProle

	/// <summary>
	/// Volusion specific account data.
	/// </summary>
	public class VolusionProle : Prole {
		#region public

		#region constructor

		public VolusionProle(ILog log, Customer customer) : base(log, customer) {} // constructor

		#endregion constructor

		#region property ShopType

		public override ShopTypes ShopType { get { return ShopTypes.Volusion; } }

		#endregion property ShopType

		#endregion public
	} // class VolusionProle

	#endregion class VolusionProle
} // namespace Integration.ChannelGrabberAPI
