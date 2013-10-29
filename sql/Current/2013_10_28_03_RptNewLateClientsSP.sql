IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptNewLateClients]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptNewLateClients]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptNewLateClients
@DateStart DATE,
@DateEnd DATE
AS
SELECT C.Id,C.Name,C.FirstName,C.Surname,AmountDue FROM LoanSchedule S,Customer C,Loan L WHERE C.IsTest = 0 AND C.Id = L.CustomerId AND S.LoanId = L.Id AND S.Date >= @DateStart AND S.Date < @DateEnd AND S.Status IN ('StillToPay','Late') AND C.CollectionStatus != 0
GO

IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type='RPT_NEW_LATE_CLIENTS') 
BEGIN
	INSERT INTO dbo.ReportScheduler(Type, Title, StoredProcedure, IsDaily, IsWeekly, IsMonthly, Header,Fields,ToEmail,IsMonthToDate) VALUES ('RPT_NEW_LATE_CLIENTS', 'New Late Clients', 'RptNewLateClients', 1, 0, 0, 'Id,Name,FirstName,Surname,AmountDue', 'Id,Name,FirstName,Surname,AmountDue', 'nimrodk@ezbob.com', 0)
	DECLARE @id INT, @reportId INT
	
	SET @reportId = (SELECT Id FROM ReportScheduler WHERE Type='RPT_NEW_LATE_CLIENTS')
	
	INSERT INTO dbo.ReportArguments	(ReportArgumentNameId, ReportId) VALUES	(1, @reportId)
	DECLARE cur CURSOR FOR SELECT Id FROM ReportUsers WHERE Name IN ('stasd', 'nimrodk')
	OPEN cur
	FETCH NEXT FROM cur INTO @id
	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM ReportsUsersMap WHERE UserID = @id AND ReportID = @reportId)
			INSERT INTO ReportsUsersMap VALUES (@id, @reportId)

		FETCH NEXT FROM cur INTO @id
	END
	CLOSE cur
	DEALLOCATE cur
END 
GO 
