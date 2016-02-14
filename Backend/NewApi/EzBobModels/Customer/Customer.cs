namespace EzBobModels.Customer {
    using System;

    public partial class Customer {

        private CustomerPersonalInfo personalInfo;
        private CustomerAddress customerAddress = new CustomerAddress();

        public CustomerPersonalInfo PersonalInfo
        {
            get { return this.personalInfo ?? (this.personalInfo = new CustomerPersonalInfo(this)); }
        }

        public CustomerAddress CustomerAddress
        {
            get {  return this.customerAddress; }
        }

        public string ABTesting { get; set; }
        public string AccountNumber { get; set; }
        public string AlibabaId { get; set; }
        public string AmlDescription { get; set; }
        public string AMLResult { get; set; }

        [Obsolete("not in use?, consider to remove")]
        public int? AmlScore { get; set; }

        public decimal AmountTaken { get; set; }
        public int? ApplyCount { get; set; }
        public string ApprovedReason { get; set; }
        public bool AvoidAutomaticDescison { get; set; }
        public DateTime? ApplyForLoan { get; set; }

        [Obsolete("not in use?, consider to remove")]
        public string BankAccountType { get; set; }

        public int? BankAccountValidationInvalidAttempts { get; set; }
        public int? BrokerID { get; set; }
        public string BWAResult { get; set; }
        public bool CciMark { get; set; }
        public int? CompanyId { get; set; }
        public string CollectionDescription { get; set; }
        public int CollectionStatus { get; set; }
        public string Comments { get; set; }
        private bool? ConsentToSearch { get; set; } // part of PersonalInfo
        public string CostumeActionItem { get; set; }
        public string CreditCardNo { get; set; }
        public string CreditResult { get; set; }

        /// <summary>
        /// The amount of offer, that is still valid for user. It is reduced on taking each new loan.
        /// </summary>
        public decimal? CreditSum { get; set; }

        public int? CurrentDebitCard { get; set; }
        public DateTime? DateApproved { get; set; }
        public DateTime? DateEscalated { get; set; }
        private DateTime? DateOfBirth { get; set; } // part of PersonalInfo
        public DateTime? DateRejected { get; set; }
        public string Details { get; set; }
        private string DaytimePhone { get; set; } // part of PersonalInfo
        public bool DefaultCardSelectionAllowed { get; set; }
        public int Id { get; set; }
        public int? ExperianConsumerScore { get; set; }
        public int? ExternalCollectionStatusID { get; set; }
        public string EmailState { get; set; }
        public string EscalationReason { get; set; }
        public bool FilledByBroker { get; set; }
        public DateTime? FirstLoanDate { get; set; }
        private string FirstName { get; set; } // part of PersonalInfo
        public string FirstVisitTime { get; set; }
        public bool? Fraud { get; set; }
        public int? FraudStatus { get; set; }
        public string Fullname { get; set; }
        private char? Gender { get; set; } // part of PersonalInfo
        public string GoogleCookie { get; set; }
        public int? GreetingMailSent { get; set; }
        public DateTime? GreetingMailSentDate { get; set; }
        public bool? HasApprovalChance { get; set; }
        private string IndustryType { get; set; } // part of PersonalInfo
        public bool IsAlibaba { get; set; }
        public bool? IsDirector { get; set; }
        public int IsLoanTypeSelectionAllowed { get; set; }
        public bool? IsOffline { get; set; }
        public bool? IsTest { get; set; }
        public bool? IsWaitingForSignature { get; set; }
        public bool? IsWasLate { get; set; }
        public decimal LastLoanAmount { get; set; }
        public DateTime? LastLoanDate { get; set; }
        public int? LastStartedMainStrategyId { get; set; }
        public DateTime? LastStartedMainStrategyEndTime { get; set; }
        public string LastStatus { get; set; }
        public decimal ManagerApprovedSum { get; set; }
        public string ManagerName { get; set; }
        private string MaritalStatus { get; set; } // part of PersonalInfo
        public string MedalType { get; set; }
        private string MiddleInitial { get; set; } //part of PersonalInfo
        private string MobilePhone { get; set; } // part of PersonalInfo
        private bool MobilePhoneVerified { get; set; } // part of PersonalInfo
        public bool? MonthlyStatusEnabled { get; set; }
        public string Name { get; set; }
        public int NumApproves { get; set; }
        public int NumRejects { get; set; }
        public int? OriginID { get; set; }
        private decimal? OverallTurnOver { get; set; } // part of PersonalInfo
        public int? PayPointErrorsCount { get; set; }
        public string PendingStatus { get; set; }
        public string PayPointTransactionId { get; set; }
        public string PromoCode { get; set; }
        public int PropertyStatusId { get; set; } // part of PersonalInfo
        public int? QuickOfferID { get; set; }
        public string ReferenceSource { get; set; }
        public string RefNumber { get; set; }
        public string RejectedReason { get; set; }
        public decimal? SetupFee { get; set; }
        public string SortCode { get; set; }
        public string Status { get; set; }
        private string Surname { get; set; } //part of PersonalInfo
        public decimal SystemCalculatedSum { get; set; }
        public string SystemDecision { get; set; }
        private int? TimeAtAddress { get; set; } // part of PersonalInfo
//        public long? TimestampCounter { get; set; }
        public decimal TotalPrincipalRepaid { get; set; }
        public int TrustPilotStatusID { get; set; }
        private string TypeOfBusiness { get; set; }
        public string UnderwriterName { get; set; }
        public DateTime? ValidFor { get; set; }
        public bool Vip { get; set; }
        public decimal? WebSiteTurnOver { get; set; } // part of PersonalInfo

        [Obsolete("not in use?, consider to remove")]
        public int? WhiteLabelId { get; set; }

        public int? WizardStep { get; set; }
    }


    /*
     
     Id	int	Unchecked
     * 

OriginID	int	Checked
ExternalCollectionStatusID	int	Checked
HasApprovalChance	bit	Checked
TimestampCounter	timestamp	Unchecked
		Unchecked
     
     */

    /*
     
     Name	nvarchar(128)	Unchecked
CreditResult	nvarchar(MAX)	Checked
CreditSum	decimal(18, 0)	Checked
GreetingMailSent	int	Checked
GreetingMailSentDate	datetime	Unchecked
Status	nvarchar(250)	Checked
AccountNumber	nvarchar(8)	Checked
SortCode	nvarchar(8)	Checked
FirstName	nvarchar(250)	Checked
MiddleInitial	nvarchar(250)	Checked
Surname	nvarchar(250)	Checked
DateOfBirth	datetime	Checked
TimeAtAddress	int	Checked
ConsentToSearch	bit	Checked
ApplyForLoan	datetime	Checked
MedalType	nvarchar(50)	Checked
PayPointTransactionId	nvarchar(250)	Checked
DateEscalated	datetime	Checked
UnderwriterName	varchar(200)	Checked
ManagerName	varchar(200)	Checked
EscalationReason	varchar(200)	Checked
DateApproved	datetime	Checked
ApplyCount	int	Checked
     * 
RejectedReason	nvarchar(1000)	Checked
Gender	char(1)	Checked
MaritalStatus	nvarchar(50)	Checked
TypeOfBusiness	nvarchar(50)	Checked
SystemDecision	nvarchar(50)	Checked
CreditCardNo	nvarchar(50)	Checked
DaytimePhone	nvarchar(50)	Checked
MobilePhone	nvarchar(50)	Checked
Fullname	nvarchar(250)	Checked
OverallTurnOver	decimal(18, 0)	Checked
WebSiteTurnOver	decimal(18, 0)	Checked
BWAResult	nvarchar(100)	Checked
AMLResult	nvarchar(100)	Checked
Fraud	bit	Checked
RefNumber	nvarchar(8)	Checked
PayPointErrorsCount	int	Checked
SetupFee	decimal(18, 0)	Checked
Comments	nvarchar(MAX)	Checked
     
     * 
Details	nvarchar(MAX)	Checked
ValidFor	datetime	Checked
CollectionStatus	int	Unchecked
ApprovedReason	nchar(200)	Checked
ReferenceSource	nvarchar(1000)	Checked
EmailState	nvarchar(100)	Checked
IsTest	bit	Checked
CurrentDebitCard	int	Checked
BankAccountType	nvarchar(50)	Checked
BankAccountValidationInvalidAttempts	int	Checked
CollectionDescription	nvarchar(500)	Checked
     * 
WizardStep	int	Checked
LastStartedMainStrategyId	int	Checked
LastStartedMainStrategyEndTime	datetime	Checked
PendingStatus	nvarchar(50)	Checked
DateRejected	datetime	Checked
IsLoanTypeSelectionAllowed	int	Checked
ABTesting	nvarchar(512)	Checked
NumApproves	int	Unchecked
NumRejects	int	Unchecked
     * 
SystemCalculatedSum	decimal(18, 4)	Unchecked
ManagerApprovedSum	decimal(18, 4)	Unchecked
FirstLoanDate	datetime	Checked
LastLoanDate	datetime	Checked
AmountTaken	decimal(18, 4)	Unchecked
LastLoanAmount	decimal(18, 4)	Unchecked
TotalPrincipalRepaid	decimal(18, 4)	Unchecked
LastStatus	nvarchar(100)	Checked
AvoidAutomaticDescison	bit	Unchecked
     * 
     * 
FraudStatus	int	Checked
IsWasLate	bit	Checked
IsOffline	bit	Checked
PromoCode	varchar(30)	Checked
MonthlyStatusEnabled	bit	Checked
CciMark	bit	Unchecked
GoogleCookie	nvarchar(300)	Checked
TrustPilotStatusID	int	Unchecked
IsDirector	bit	Checked
IndustryType	nvarchar(50)	Checked
QuickOfferID	int	Checked
CompanyId	int	Checked
BrokerID	int	Checked
FilledByBroker	bit	Unchecked
     * 

Vip	bit	Unchecked
DefaultCardSelectionAllowed	bit	Unchecked
FirstVisitTime	nvarchar(64)	Checked
AmlDescription	nvarchar(200)	Checked
AmlScore	int	Checked
ExperianConsumerScore	int	Checked
PropertyStatusId	int	Checked
MobilePhoneVerified	bit	Unchecked
WhiteLabelId	int	Checked
IsWaitingForSignature	bit	Checked
CostumeActionItem	nvarchar(1000)	Checked
IsAlibaba	bit	Unchecked
AlibabaId	nvarchar(300)	Checked
     */

}
