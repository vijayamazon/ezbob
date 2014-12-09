using System;
using System.Xml;
using Integration.ChannelGrabberConfig;

namespace Integration.ChannelGrabberAPI {

	public class Order {

		public static Order Create(XmlNode oNode, string sShopTypeName, AccountData oAccountData, int nIsExpense) {
			string sOrderShopTypeName = XmlUtil.GetString(oNode, "accountType");

			if (sOrderShopTypeName.ToLower() != sShopTypeName)
				return null;

			int nAccountID = XmlUtil.GetInt(oNode, "accountId");

			if (nAccountID != oAccountData.Id())
				return null;

			return new Order(oNode, nIsExpense);
		} // Create

		public virtual string   NativeOrderId { get; set; }
		public virtual double?  TotalCost     { get; set; }
		public virtual string   CurrencyCode  { get; set; }
		public virtual DateTime PaymentDate   { get; set; }
		public virtual DateTime PurchaseDate  { get; set; }
		public virtual string   OrderStatus   { get; set; }
		public virtual int      IsExpense     { get; set; }

		private Order(XmlNode oNode, int nIsExpense) {
			NativeOrderId = XmlUtil.GetString(oNode, XmlUtil.IdNode);
			TotalCost     = XmlUtil.GetDouble(oNode, "total");
			CurrencyCode  = XmlUtil.GetString(oNode, "currency");
			PaymentDate   = XmlUtil.GetDate(oNode, "paymentDate");
			PurchaseDate  = XmlUtil.GetDate(oNode, "purchaseDate");
			OrderStatus   = XmlUtil.GetString(oNode, "status");
			IsExpense     = nIsExpense;
		} // constructor

	} // class Order

} // namespace Integration.ChannelGrabberAPI
