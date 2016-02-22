namespace EzBob.Web.Areas.Customer.Models {
	using System;

	public class RequestedLoanModel {
		public int Id { get; set; }
		public DateTime Created { get; set; }
		public int CustomerId { get; set; }
		public double? Amount { get; set; }
		public int? Term { get; set; }
		public decimal MinLoanAmount { get; set; }
		public decimal MaxLoanAmount { get; set; }
		public int MinTerm { get; set; }
		public int MaxTerm { get; set; }
	}
}