namespace IMailLib {
	using System;
	using System.Collections.Generic;
	using Aspose.Words.Lists;

	public class Address {
		public string Line1 { get; set; }
		public string Line2 { get; set; }
		public string Line3 { get; set; }
		public string Line4 { get; set; }
		public string Postcode { get; set; }
	}

	public class CollectionMailModel {
		public Address CompanyAddress { get; set; }
		public string CompanyName { get; set; }
		public Address CustomerAddress { get; set; }
		public int CustomerId { get; set; }
		public int OriginId { get; set; }
		public string CustomerName { get; set; }
		public DateTime Date { get; set; }
		public Address GuarantorAddress { get; set; }
		public string GuarantorName { get; set; }
		public bool IsLimited { get; set; }
		public int LoanAmount { get; set; }
		public DateTime LoanDate { get; set; }
		public string LoanRef { get; set; }
		public decimal MissedInterest { get; set; }
		public MissedPaymentModel MissedPayment { get; set; }
		public decimal OutstandingBalance { get; set; }
		public decimal OutstandingPrincipal { get; set; }
		public MissedPaymentModel PreviousMissedPayment { get; set; }
	}

	public class MissedPaymentModel {
		public decimal AmountDue { get; set; }
		public DateTime DateDue { get; set; }
		public decimal Fees { get; set; }
		public decimal RepaidAmount { get; set; }
		public DateTime? RepaidDate { get; set; }
	}

	public class TableModel {
		public List<string> Header { get; set; }
		public List<List<string>> Content { get; set; }
	}
}
