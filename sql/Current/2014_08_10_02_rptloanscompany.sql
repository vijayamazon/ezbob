DECLARE @Type NVARCHAR(100) = 'RPT_LOANS_COMPANY'
DECLARE @Title NVARCHAR(100) = 'Loans company'
DECLARE @Sp NVARCHAR(100) = 'RptLoansCompany'
DECLARE @Emails NVARCHAR(200) = ''
DECLARE @Headers NVARCHAR(200) = 'CustomerId,Full name, Email,Company Name,Experian Company Name,Experian Ref Num,Loan Ref Num'
DECLARE @Fields NVARCHAR(200) = '#CustomerId,Fullname, Email,CompanyName,ExperianCompanyName,ExperianRefNum,LoanRefNum'

IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = @Type)
BEGIN
       INSERT INTO ReportScheduler(Type,  Title,  StoredProcedure, IsDaily, IsWeekly, IsMonthly, IsMonthToDate, Header,   Fields,  ToEmail)
       VALUES                     (@Type, @Title, @Sp,                 0,       0,        0,         0,             @Headers, @Fields,     @Emails)

	   INSERT INTO ReportArguments (ReportArgumentNameId, ReportId)
	   SELECT 1, Id FROM ReportScheduler WHERE     Type = @Type
		
	   INSERT INTO ReportsUsersMap (UserID, ReportID)
	         SELECT u.Id, r.Id
		     FROM ReportUsers u,     ReportScheduler r
		     WHERE
		          u.UserName IN ('alexbo', 'stasd', 'nimrodk', 'eilaya', 'emanuellea')
		          AND r.Type = @Type AND NOT EXISTS (SELECT *     FROM ReportsUsersMap WHERE UserID = u.Id AND ReportID = r.ID)
END
GO
