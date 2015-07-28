namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class BrokerInvoiceCommissionModel {

		[DataMember]
		public int BrokerID { get; set; }
		[DataMember]
		public DateTime CommissionTime { get; set; }
		[DataMember]
		public string CustomerName { get; set; }
		[DataMember]
		public decimal CommissionAmount { get; set; }
		[DataMember]
		public string SortCode { get; set; }
		[DataMember]
		public string BankAccount { get; set; }
		[DataMember]
		public int InvoiceID { get; set; }

	} // class BrokerInvoiceCommissionModel
} // namespace Ezbob.Backend.Models
