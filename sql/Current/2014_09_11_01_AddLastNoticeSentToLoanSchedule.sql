IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'LastNoticeSent' and Object_ID = Object_ID(N'LoanSchedule'))    
BEGIN
	ALTER TABLE LoanSchedule ADD LastNoticeSent BIT
	
	DECLARE @Statement NVARCHAR(MAX)
	
	SET @Statement = 'UPDATE LoanSchedule SET LastNoticeSent = 0'
	
	EXEC(@Statement)
END
GO

