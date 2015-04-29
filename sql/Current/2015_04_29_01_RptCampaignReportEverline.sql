SET QUOTED_IDENTIFIER ON
GO

DECLARE @Type NVARCHAR(100) = 'RPT_CAMPAIGN_REPORT_EVERLINE'
DECLARE @Title NVARCHAR(100) = 'Campaign traffic everline'
DECLARE @Sp NVARCHAR(100) = 'RptCampaignReportEverline'
DECLARE @Emails NVARCHAR(200) = 'stasd+report@ezbob.com,sivanc@ezbob.com,maayan@yellowhead.pro,eran@yellowhead.pro,gal@yellowheadinc.com,maya@yellowheadinc.com'
DECLARE @Headers NVARCHAR(200) = 'Source,Medium,Term,Name,Registrations,Personal,Company,DataSources,Applications,NumOfApproved,NumOfRejected,RequestedAmount,NumOfLoans,LoanAmount'
DECLARE @Fields NVARCHAR(200) = 'Source,Medium,Term,Name,Registrations,Personal,Company,DataSources,Applications,NumOfApproved,NumOfRejected,RequestedAmount,NumOfLoans,LoanAmount'
DECLARE @IsDaily BIT = 1
DECLARE @IsWeekly BIT = 1
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
	          u.UserName IN ('alexbo', 'stasd', 'sivanc')
	          AND r.Type = @Type AND NOT EXISTS (SELECT * FROM ReportsUsersMap WHERE UserID = u.Id AND ReportID = r.ID)
	END
GO