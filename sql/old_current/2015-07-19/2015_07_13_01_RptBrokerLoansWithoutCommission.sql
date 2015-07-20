DECLARE @Type NVARCHAR(100) = 'RPT_BROKER_LOANS_WITHOUT_COMMISSION'
DECLARE @Title NVARCHAR(100) = 'Broker loans without commission'
DECLARE @Sp NVARCHAR(100) = 'RptBrokerLoansWithoutCommission'
DECLARE @Emails NVARCHAR(200) = ''
DECLARE @Headers NVARCHAR(200) = 'Id,CustomerId,BrokerID,Date,LoanAmount,SetupFee,IdUnderwriter,BrokerSetupFeePercent,ManualSetupFeePercent'
DECLARE @Fields NVARCHAR(200) = '!Id,#CustomerId,^BrokerID,Date,LoanAmount,SetupFee,!IdUnderwriter,%BrokerSetupFeePercent,%ManualSetupFeePercent'
DECLARE @IsDaily BIT = 0
DECLARE @IsWeekly BIT = 0
DECLARE @IsMonthly BIT = 0
DECLARE @IsMonthToDate BIT = 0

IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = @Type)
BEGIN
           INSERT INTO ReportScheduler(Type,  Title,  StoredProcedure, IsDaily, IsWeekly, IsMonthly, IsMonthToDate, Header,   Fields,  ToEmail)
           VALUES                     (@Type, @Title, @Sp,             @IsDaily,@IsWeekly,@IsMonthly,@IsMonthToDate,@Headers, @Fields, @Emails)

INSERT INTO ReportArguments (ReportArgumentNameId, ReportId)
SELECT 1, Id FROM ReportScheduler WHERE Type = @Type

INSERT INTO ReportsUsersMap (UserID, ReportID)
     SELECT u.Id, r.Id
     FROM ReportUsers u,     ReportScheduler r
     WHERE
          u.UserName IN ('alexbo', 'stasd', 'travism')
          AND r.Type = @Type AND NOT EXISTS (SELECT * FROM ReportsUsersMap WHERE UserID = u.Id AND ReportID = r.ID)
END
GO