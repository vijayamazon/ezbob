DECLARE @Type NVARCHAR(100) = 'RPT_CRM_FOLLOWUP'
DECLARE @Title NVARCHAR(100) = 'CRM Follow up'
DECLARE @Sp NVARCHAR(100) = 'RptCrmFollowUp'
DECLARE @Emails NVARCHAR(200) = 'rosb@ezbob.com'
DECLARE @Headers NVARCHAR(200) = 'CustomerId,Email,FollowUpDate,Comment'
DECLARE @Fields NVARCHAR(200) = '#CustomerId,Email,FollowUpDate,Comment'

IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = @Type)
BEGIN
	INSERT INTO ReportScheduler(Type,  Title,  StoredProcedure, IsDaily, IsWeekly, IsMonthly, IsMonthToDate, Header,   Fields,  ToEmail) 
	VALUES                     (@Type, @Title, @Sp,	            1,       0,        0,         0,             @Headers, @Fields,	@Emails)

	INSERT INTO ReportArguments (ReportArgumentNameId, ReportId)
	SELECT 1, Id FROM ReportScheduler WHERE	Type = @Type

	INSERT INTO ReportsUsersMap (UserID, ReportID)
		SELECT u.Id, r.Id
		FROM ReportUsers u,	ReportScheduler r
		WHERE
			u.UserName IN ('alexbo', 'stasd', 'nimrodk', 'eilaya', 'rosb')
			AND r.Type = @Type AND NOT EXISTS (SELECT *	FROM ReportsUsersMap WHERE UserID = u.Id AND ReportID = r.ID)
END
GO
