namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System;
	using System.Runtime.Serialization;

	[DataContract]
	public class MonthlyRepaymentData {
		[DataMember]
		public decimal RequestedAmount { get; set; }

		[DataMember]
		public int RequestedTerm { get; set; }

		[DataMember]
		public decimal MonthlyRepayment { get; set; }

		public int MonthlyPayment {
			get {
				if (MonthlyRepayment > Int32.MaxValue)
					return Int32.MaxValue;

				if (MonthlyRepayment <= 0)
					return 0;

				return (int)MonthlyRepayment;
			} // get
		} // MonthlyPayment
	} // class MonthlyRepaymentData
} // namespace
