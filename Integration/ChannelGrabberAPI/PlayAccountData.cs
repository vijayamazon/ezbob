using System.Xml;

namespace Integration.ChannelGrabberAPI {
	#region class PlayAccountData

	public class PlayAccountData : AAccountData {
		#region public

		#region public properties for JSON

		public string name     { get; set; }
		public string username { get; set; }
		public string password { get; set; }

		#endregion public properties for JSON

		#region method ToString

		public override string ToString() {
			return string.Format("{0} as {1}", name, username);
		} // ToString

		#endregion method ToString

		#endregion public

		#region protected

		#region method IsMe

		protected override bool IsMe(XmlNode oNode) {
			return
				(API.GetString(oNode, "name") == name) &&
				(API.GetString(oNode, "username") == username);
		} // IsMe

		#endregion method IsMe

		#endregion protected
	} // class PlayAccountData

	#endregion class PlayAccountData
} // namespace Integration.ChannelGrabberAPI
