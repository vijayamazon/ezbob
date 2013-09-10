IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type='RPT_LOAN_INTEGRITY')
BEGIN
	INSERT INTO dbo.ReportScheduler VALUES ('RPT_LOAN_INTEGRITY', 'Loan Integrity', 'No SP', 0, 0, 0, 'Loan Id, Diff', 'LoanID,Diff', 'yulys@ezbob.com', 0)

	DECLARE @id INT,
			@reportId INT
			
	SELECT @reportId = Id FROM ReportScheduler WHERE Type = 'RPT_LOAN_INTEGRITY'

	DECLARE cur CURSOR FOR SELECT Id FROM ReportUsers WHERE Name IN ('yulys', 'nimrodk')
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


