namespace Reports.TraficReport
{
	public class TraficReportRow
	{
		public Source Channel { get; set; }
		public int Visits { get; set; }
		public int Visitors { get; set; }
		public int Registrations { get; set; }
		public string Applications { get; set; } //todo make int and retrieve
		public int Loans { get; set; }
		public int LoanAmount { get; set; }
		public string Cost { get; set; } //todo make int and retrieve
		public string NewCustomerCost { get; set; } //todo make int and retrieve
		public string ROI { get; set; } //todo make int and retrieve
	}
}
