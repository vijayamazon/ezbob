IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Title = 'RPT_NEW_CCI')
BEGIN
	INSERT INTO ReportScheduler
		(Type, Title, StoredProcedure, IsDaily, IsWeekly, IsMonthly, Header, Fields, ToEmail, IsMonthToDate)
	VALUES
		('RPT_NEW_CCI', 'New cci status - all customers', 'RptNewCci', 0, 0, 0, 'CustomerId,FirstName,Surname,Email,IsBroker,LoanId,LoanAmount,PrincipalOwed,IsEu', 'CustomerId,FirstName,Surname,Email,IsBroker,LoanId,LoanAmount,PrincipalOwed,IsEu', '', 0)
		
	DECLARE @UserId INT, @ReportId INT, @ReportArgumentNameId INT
	SELECT @UserId = Id FROM ReportUsers WHERE UserName = 'emanuellea'
	SELECT @ReportId = Id FROM ReportScheduler WHERE Type = 'RPT_NEW_CCI'
	SELECT @ReportArgumentNameId = Id FROM ReportArgumentNames WHERE NAme = 'DateRange'
	
	INSERT INTO ReportsUsersMap
		(UserID, ReportID)
	VALUES
		(@UserId, @ReportId)
		
	INSERT INTO ReportArguments
		(ReportArgumentNameId, ReportId)
	VALUES
		(@ReportArgumentNameId, @ReportId)
END
GO
