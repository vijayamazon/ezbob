using System;

namespace Integration.ChannelGrabberConfig {
	#region class ClientSide

	public class ClientSide : ICloneable {
		#region public

		#region constructor

		public ClientSide() {
			StoreInfoStepModelShops = "";
			LinkForm = new LinkForm();
			SortPriority = 0;
			IsNew = false;
		} // constructor

		#endregion constructor

		#region properties

		public string StoreInfoStepModelShops { get; set; }
		public LinkForm LinkForm { get; set; }
		public int SortPriority { get; set; }
		public bool IsNew { get; set; }

		#endregion properties

		#region method Clone

		public object Clone() {
			return new ClientSide {
				StoreInfoStepModelShops = (string)this.StoreInfoStepModelShops.Clone(),
				LinkForm = (LinkForm)this.LinkForm.Clone(),
				SortPriority = this.SortPriority,
				IsNew = this.IsNew
			};
		} // Clone

		#endregion method Clone

		#endregion public
	} // class ClientSide

	#endregion class ClientSide
} // namespace Integration.ChannelGrabberConfig
