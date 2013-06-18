using System;
using System.Xml;
using Integration.ChannelGrabberConfig;

namespace Integration.ChannelGrabberAPI {
	#region class Order

	public class Order {
		#region public

		#region method Create

		public static Order Create(XmlNode oNode, string sShopTypeName, AccountData oAccountData) {
			string sOrderShopTypeName = XmlUtil.GetString(oNode, "accountType");

			if (sOrderShopTypeName.ToLower() != sShopTypeName)
				return null;

			int nAccountID = XmlUtil.GetInt(oNode, "accountId");

			if (nAccountID != oAccountData.Id())
				return null;

			return new Order(oNode);
		} // Create

		#endregion method Create

		#region public properties

		public virtual string   NativeOrderId { get; set; }
		public virtual double?  TotalCost     { get; set; }
		public virtual string   CurrencyCode  { get; set; }
		public virtual DateTime PaymentDate   { get; set; }
		public virtual DateTime PurchaseDate  { get; set; }
		public virtual string   OrderStatus   { get; set; }

		#endregion public properties

		#endregion public

		#region private

		#region constructor

		private Order(XmlNode oNode) {
			NativeOrderId = XmlUtil.GetString(oNode, XmlUtil.IdNode);
			TotalCost     = XmlUtil.GetDouble(oNode, "total");
			CurrencyCode  = XmlUtil.GetString(oNode, "currency");
			PaymentDate   = XmlUtil.GetDate(oNode, "paymentDate");
			PurchaseDate  = XmlUtil.GetDate(oNode, "purchaseDate");
			OrderStatus   = XmlUtil.GetString(oNode, "status");
		} // constructor

		#endregion constructor

		#endregion private
	} // class Order

	#endregion class Order
} // namespace Integration.ChannelGrabberAPI
