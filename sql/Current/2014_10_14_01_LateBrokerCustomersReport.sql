IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Title = 'RPT_BROKER_LATE_CLIENTS')
BEGIN
	INSERT INTO ReportScheduler
		(Type, Title, StoredProcedure, IsDaily, IsWeekly, IsMonthly, Header, Fields, ToEmail, IsMonthToDate)
	VALUES
		('RPT_BROKER_LATE_CLIENTS', 'Late broker customers', 'RptBrokerLateClients', 0, 0, 0, 'Id,FirmName,Name,FirstName,Surname,PrincipalOwed', 'Id,FirmName,Name,FirstName,Surname,PrincipalOwed', '', 0)
		
	DECLARE @UserId INT, @ReportId INT, @ReportArgumentNameId INT
	SELECT @UserId = Id FROM ReportUsers WHERE UserName = 'emanuellea'
	SELECT @ReportId = Id FROM ReportScheduler WHERE Type = 'RPT_BROKER_LATE_CLIENTS'
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
