namespace Ezbob.Backend.Models {
	using System;
	using DbConstants;

	public enum CollectionMethod {
		Email,
		Mail,
		Sms,
		ChangeStatus
	}

	//enum

	public enum CollectionType {
		Cured,
		CollectionDay0,
		CollectionDay1to6,
		CollectionDay3,
		CollectionDay7,
		CollectionDay10,
		CollectionDay13,
		CollectionDay8to14,
		CollectionDay15,
		CollectionDay21,
		CollectionDay31,
		CollectionDay46,
		CollectionDay60,
		CollectionDay90,
		Annual77ANotification
	}

	//enum

	public class CollectionDataModel {
		public decimal AmountDue { get; set; }
		public int CustomerID { get; set; }
		public int OriginID { get; set; }
		public DateTime DueDate { get; set; }
		public string Email { get; set; }
		public bool EmailSendingAllowed { get; set; }
		public decimal FeeAmount { get; set; }
		public string FirstName { get; set; }
		public string FullName { get; set; }
		public bool ImailSendingAllowed { get; set; }
		public decimal Interest { get; set; }
		public int LateDays { get; set; }
		public int LoanID { get; set; }
		public string LoanRefNum { get; set; }
		public string PhoneNumber { get; set; }
		public int ScheduleID { get; set; }
		public bool SmsSendingAllowed { get; set; }
		public long LoanHistoryID { get; set; }
		public long NLLoanID { get; set; }
		public long NLScheduleID { get; set; }
		public bool UpdateCustomerAllowed { get; set; }
		public override string ToString() {
			return String.Format(@"Collection model for 
                                    CustomerID:{0}, OriginID: {17}
                                    LoanID:{1}, NLLoanID:{18}, NLScheduleID:{19}, UpdateCustomerAllowed:{20}, LoanHistoryID:{21}, LoanRefNum:{11},
                                    ScheduleID:{2},
                                    Email:{6},FirstName:{3}{4},FullName:{5}, 
                                    PhoneNumber:{7}
                                    AmountDue:{8},FeeAmount:{9},Interest:{10}, 
                                    DueDate:{12},
                                    LateDays: {13}
                                    EmailSendingAllowed:{14}, SmsSendingAllowed: {15}, ImailSendingAllowed: {16}",
				CustomerID, LoanID, ScheduleID,
				FirstName, "", FullName, Email, PhoneNumber, AmountDue, FeeAmount, Interest, LoanRefNum, DueDate, LateDays,
				EmailSendingAllowed, SmsSendingAllowed, ImailSendingAllowed, OriginID, NLLoanID, NLScheduleID, UpdateCustomerAllowed, LoanHistoryID);
		}
	}

	//class CollectionDataModel

	public class LoanStatsModel {
		public decimal Late30 { get; set; }
		public int Late30Num { get; set; }
		public decimal Late60 { get; set; }
		public int Late60Num { get; set; }
		public decimal Late90 { get; set; }
		public int Late90Num { get; set; }
		public decimal Late90Plus { get; set; }
		public int Late90PlusNum { get; set; }
		public decimal PastDues { get; set; }
		public int PastDuesNum { get; set; }
	}

	public class NLLateLoansJobModel {
		public long LoanScheduleID { get; set; }
		public long LoanID { get; set; }
		public int OldLoanID { get; set; }
		public int CustomerID { get; set; }
		public NLLoanStatuses LoanStatus { get; set; }
		public NLScheduleStatuses ScheduleStatus { get; set; }
		public DateTime PlannedDate { get; set; }
	}
}
