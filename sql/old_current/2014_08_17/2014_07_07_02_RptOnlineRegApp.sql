
DECLARE @Type NVARCHAR(100) = 'RPT_ONLINE_REG_APP'
DECLARE @Title NVARCHAR(100) = 'online/offline reg app'
DECLARE @Sp NVARCHAR(100) = 'RptOnlineRegApp'
DECLARE @Emails NVARCHAR(200) = ''
DECLARE @Headers NVARCHAR(200) = 'Caption,RegDate,Amount,Css'
DECLARE @Fields NVARCHAR(200) = 'Caption,RegDate,Amount,{Css'

IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = @Type)
BEGIN
           INSERT INTO ReportScheduler(Type,  Title,  StoredProcedure, IsDaily, IsWeekly, IsMonthly, IsMonthToDate, Header,   Fields,  ToEmail)
           VALUES                     (@Type, @Title, @Sp,                 1,       0,        0,         0,             @Headers, @Fields,     @Emails)

INSERT INTO ReportArguments (ReportArgumentNameId, ReportId)
SELECT 1, Id FROM ReportScheduler WHERE     Type = @Type

INSERT INTO ReportsUsersMap (UserID, ReportID)
     SELECT u.Id, r.Id
     FROM ReportUsers u,     ReportScheduler r
     WHERE
          u.UserName IN ('alexbo', 'stasd', 'nimrodk', 'eilaya', 'adic', 'galitg')
          AND r.Type = @Type AND NOT EXISTS (SELECT *     FROM ReportsUsersMap WHERE UserID = u.Id AND ReportID = r.ID)
END
GO

