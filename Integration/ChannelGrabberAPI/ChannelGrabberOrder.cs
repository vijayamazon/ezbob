using System;
using System.Xml;

namespace Integration.ChannelGrabberAPI {
	#region class ChannelGrabberOrder

	public class ChannelGrabberOrder {
		#region public

		#region constructor

		public ChannelGrabberOrder(XmlNode oNode) {
			NativeOrderId = API.GetString(oNode, API.IdNode);
			TotalCost     = API.GetDouble(oNode, "total");
			CurrencyCode  = API.GetString(oNode, "currency");
			PaymentDate   = API.GetDate(oNode, "paymentDate");
			PurchaseDate  = API.GetDate(oNode, "purchaseDate");
			OrderStatus   = API.GetString(oNode, "status");
		} // constructor

		#endregion constructor

		#region public properties

		public virtual string   NativeOrderId { get; set; }
		public virtual double?  TotalCost     { get; set; }
		public virtual string   CurrencyCode  { get; set; }
		public virtual DateTime PaymentDate   { get; set; }
		public virtual DateTime PurchaseDate  { get; set; }
		public virtual string   OrderStatus   { get; set; }

		#endregion public properties

		#endregion public

		#region protected
		#endregion protected

		#region private
		#endregion private
	} // class ChannelGrabberOrder

	#endregion class ChannelGrabberOrder
} // namespace Integration.ChannelGrabberAPI
