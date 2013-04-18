using System.Xml;

namespace Integration.ChannelGrabberAPI {
	#region class VolusionAccountData

	public class VolusionAccountData : AAccountData {
		#region public

		#region public properties for JSON

		public string name     { get; set; }
		public string endpoint { get; set; }
		public string username { get; set; }
		public string password { get; set; }

		#endregion public properties for JSON

		#region method ToString

		public override string ToString() {
			return string.Format("{0} ({1} @ {2})", name, username, endpoint);
		} // ToString

		#endregion method ToString

		#endregion public

		#region protected

		#region method IsMe

		protected override bool IsMe(XmlNode oNode) {
			return
				(API.GetString(oNode, "endpoint") == endpoint) &&
				(API.GetString(oNode, "username") == username);
		} // IsMe

		#endregion method IsMe

		#endregion protected
	} // class VolusionAccountData

	#endregion class VolusionAccountData
} // namespace Integration.ChannelGrabberAPI
