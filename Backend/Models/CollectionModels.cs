namespace Ezbob.Backend.Models
{
    using System;
    public enum CollectionMethod
    {
        Email,
        Mail,
        Sms,
        ChangeStatus
    }

    //enum

    public enum CollectionType
    {
        Cured,
        CollectionDay0,
        CollectionDay1to6,
        CollectionDay7,
        CollectionDay8to14,
        CollectionDay15,
        CollectionDay21,
        CollectionDay31,
        CollectionDay46,
        CollectionDay60,
        CollectionDay90
    }

    //enum

    public class CollectionDataModel
    {
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
        public int LoanHistoryID { get; set; }
        public string LoanRefNum { get; set; }
        public string PhoneNumber { get; set; }
        public int ScheduleID { get; set; }
        public bool SmsSendingAllowed { get; set; }
        public override string ToString()
        {
            return String.Format(@"Collection model for 
                                    CustomerID:{0}, OriginID: {17}
                                    LoanID:{1},LoanRefNum:{11},
                                    ScheduleID:{2},
                                    Email:{6},FirstName:{3}{4},FullName:{5}, 
                                    PhoneNumber:{7}
                                    AmountDue:{8},FeeAmount:{9},Interest:{10}, 
                                    DueDate:{12},
                                    LateDays: {13}
                                    EmailSendingAllowed:{14}, SmsSendingAllowed: {15}, ImailSendingAllowed: {16}",
                CustomerID, LoanID, ScheduleID,
                FirstName, "", FullName, Email, PhoneNumber, AmountDue, FeeAmount, Interest, LoanRefNum, DueDate, LateDays,
                EmailSendingAllowed, SmsSendingAllowed, ImailSendingAllowed, OriginID);
        }
    }

    //class CollectionDataModel

    public class LoanStatsModel
    {
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

    //class LoanStatsModel
}
