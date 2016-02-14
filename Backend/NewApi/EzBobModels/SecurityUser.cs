namespace EzBobModels {
    using System;

    /// <summary>
    /// Represents Security_User table
    /// </summary>
    public class SecurityUser {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsDeleted { get; set; }
        public string EMail { get; set; }
        public int? CreateUserId { get; set; }
        public DateTime? DeletionDate { get; set; }
        public int? DeleteUserId { get; set; }
        /// <summary>
        /// 0: Customer or broker
        /// 1: Underwriter
        /// 2: Investor
        /// </summary>
        /// <value>
        /// The branch identifier.
        /// </value>
        public int BranchId { get; set; }
        public DateTime? PassSetTime { get; set; }
        public int? LoginFailedCount { get; set; }
        public DateTime? DisableDate { get; set; }
        public DateTime? LastBadLogin { get; set; }
        public long? PassExpPeriod { get; set; }
        public int? ForcePassChange { get; set; }
        public int? DisablePassChange { get; set; }
        public int? DeleteId { get; set; }
        public string CertificateThumbprint { get; set; }
        public string DomainUserName { get; set; }
        public long? SecurityQuestion1Id { get; set; }
        public string SecurityAnswer1 { get; set; }
        public bool? IsPasswordRestored { get; set; }
        public string EzPassword { get; set; }
        public int? EmailStateID { get; set; }
        public string Salt { get; set; }
        public string CycleCount { get; set; }
    }

    /*
     
     UserId	int	Unchecked
UserName	nvarchar(250)	Unchecked
FullName	nvarchar(250)	Unchecked
Password	nvarchar(200)	Checked
CreationDate	datetime	Unchecked
IsDeleted	int	Unchecked
EMail	nvarchar(255)	Checked
CreateUserId	int	Checked
DeletionDate	datetime	Checked
DeleteUserId	int	Checked
BranchId	int	Unchecked
PassSetTime	datetime	Checked
LoginFailedCount	int	Checked
DisableDate	datetime	Checked
LastBadLogin	datetime	Checked
PassExpPeriod	bigint	Checked
ForcePassChange	int	Checked
DisablePassChange	int	Checked
DeleteId	int	Checked
CertificateThumbprint	nvarchar(40)	Checked
DomainUserName	nvarchar(250)	Checked
SecurityQuestion1Id	bigint	Checked
SecurityAnswer1	varchar(200)	Checked
IsPasswordRestored	bit	Checked
EzPassword	varchar(255)	Checked
EmailStateID	int	Checked
OriginID	int	Checked
Salt	varchar(255)	Checked
CycleCount	varchar(255)	Checked
TimestampCounter	timestamp	Unchecked
     
     */
}
