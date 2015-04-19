namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataAccs {
		public CallCreditDataAccs() {
			AccHistory = new List<CallCreditDataAccsHistory>();
			AccNocs = new List<CallCreditDataAccsNocs>();
	}
		[PK]
		[NonTraversable]
		public long CallCreditDataAccsID { get; set; }
		[FK("CallCreditData", "CallCreditDataID")]
		public long? CallCreditDataID { get; set; }

		public int? OiaID { get; set; }
		[Length(164)]
		public string AccHolderName { get; set; }
		public DateTime? Dob { get; set; }
		[Length(10)]
		public string StatusCode { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public bool? CurrentAddress { get; set; }
		public int? UnDeclaredAddressType { get; set; }
		[Length(440)]
		public string AddressValue { get; set; }
		public DateTime? DefDate { get; set; }
		public int? OrigDefBal { get; set; }
		public int? TermBal { get; set; }
		public DateTime? DefSatDate { get; set; }
		public DateTime? RepoDate { get; set; }
		public DateTime? DelinqDate { get; set; }
		public int? DelinqBal { get; set; }
		[Length(20)]
		public string AccNo { get; set; }
		public int? AccSuffix { get; set; }
		public int? Joint { get; set; }
		[Length(10)]
		public string Status { get; set; }
		public DateTime? DateUpdated { get; set; }
		[Length(10)]
		public string AccTypeCode { get; set; }
		public int? AccGroupId { get; set; }
		[Length(10)]
		public string CurrencyCode { get; set; }
		public int? Balance { get; set; }
		public int? CurCreditLimit { get; set; }
		public int? OpenBalance { get; set; }
		public DateTime? ArrStartDate { get; set; }
		public DateTime? ArrEndDate { get; set; }
		public DateTime? PayStartDate { get; set; }
		public DateTime? accStartDate { get; set; }
		public DateTime? AccEndDate { get; set; }
		public int? RegPayment { get; set; }
		public int? ExpectedPayment { get; set; }
		public int? ActualPayment { get; set; }
		public int? RepayPeriod { get; set; }
		[Length(10)]
		public string RepayFreqCode { get; set; }
		public int? LumpPayment { get; set; }
		public int? PenIntAmt { get; set; }
		public bool? PromotionalRate { get; set; }
		public bool? MinimumPayment { get; set; }
		public int? StatementBalance { get; set; }
		[Length(60)]
		public string SupplierName { get; set; }
		[Length(10)]
		public string SupplierTypeCode { get; set; }
		public bool? Apacs { get; set; }

		[NonTraversable]
		public List<CallCreditDataAccsHistory> AccHistory { get; set; }
		[NonTraversable]
		public List<CallCreditDataAccsNocs> AccNocs { get; set; }

	}
}
