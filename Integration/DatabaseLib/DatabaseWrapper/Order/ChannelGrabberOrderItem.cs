using System;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order {
	public class ChannelGrabberOrderItem : AInternalOrderItem {
		#region public

		#region enum TypeName

		public enum TypeName {
			Other,
			Order,
			Expense,
		} // enum TypeName

		#endregion enum TypeName

		public virtual double? TotalCost { get; set; }
		public virtual string CurrencyCode { get; set; }
		public virtual DateTime PaymentDate { get; set; }
		public virtual DateTime PurchaseDate { get; set; }
		public virtual string OrderStatus { get; set; }
		public virtual int IsExpense { get; set; }

		public override DateTime RecordTime { get { return PurchaseDate; }} // RecordTime

		#region method GetTypeName

		public virtual TypeName GetTypeName() {
			if (!TotalCost.HasValue)
				return TypeName.Other;

			// if (OrderStatus != "Dispatched")
			// 	return TypeName.Other;

			switch (IsExpense) {
			case 0:
				return TypeName.Order;

			case 1:
				return TypeName.Expense;

			default:
				return TypeName.Other;
			} // switch
		} // GetTypeName

		#endregion method GetTypeName

		#region method Convert

		public virtual double Convert(ICurrencyConvertor cc) {
			if (!TotalCost.HasValue)
				return 0;

			return cc.ConvertToBaseCurrency(
				CurrencyCode,
				(double)TotalCost,
				PurchaseDate
			).Value;
		} // Convert

		#endregion method Convert

		#endregion public
	} // class ChannelGrabberOrderItem
} // namespace
