DECLARE @Type NVARCHAR(100) = 'RPT_CHANNEL_MARKETING'
DECLARE @Title NVARCHAR(100) = 'Channel Marketing'
DECLARE @Sp NVARCHAR(100) = 'RptChannelMarketing'
DECLARE @Emails NVARCHAR(200) = 'yarons@ezbob.com,tomerg@ezbob.com,alexbo+rpt@ezbob.com,stasd+rpt@ezbob.com,travism@ezbob.com,rosb@ezbob.com,orana@ezbob.com,sivanc@ezbob.com'
DECLARE @Headers NVARCHAR(200) = 'Source,Medium,Registrations,Personal,Company,DataSources,Applications,NumOfApproved,NumOfRejected,RequestedAmount,NumOfLoans,LoanAmount'
DECLARE @Fields NVARCHAR(200) = 'Source,Medium,Registrations,Personal,Company, DataSources,Applications,NumOfApproved,NumOfRejected,RequestedAmount,NumOfLoans,LoanAmount'
DECLARE @IsDaily BIT = 0
DECLARE @IsWeekly BIT = 0
DECLARE @IsMonthly BIT = 0
DECLARE @IsMonthToDate BIT = 1

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
          u.UserName IN ('alexbo', 'stasd', 'eilaya', 'rosb' , 'sivanc', 'tomerg')
          AND r.Type = @Type AND NOT EXISTS (SELECT * FROM ReportsUsersMap WHERE UserID = u.Id AND ReportID = r.ID)
END
GO
