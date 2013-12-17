using System;

namespace CommonLib
{
	public class ReRejectionData
	{
		public DateTime? ManualRejectDate { get; set; }
		public bool IsNewClient { get; set; }
		public bool NewDataSourceAdded { get; set; }
		public decimal RepaidAmount { get; set; }
		public int LoanAmount { get; set; }
	}
}
