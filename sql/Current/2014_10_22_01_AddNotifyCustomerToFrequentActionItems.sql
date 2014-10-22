IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'MailToCustomer' and Object_ID = Object_ID(N'FrequentActionItems'))    
BEGIN
	ALTER TABLE FrequentActionItems ADD MailToCustomer BIT
	
	DECLARE @Statement NVARCHAR(MAX)
	
	SET @Statement = 'UPDATE FrequentActionItems SET MailToCustomer = 1'
	SET @Statement = 'UPDATE FrequentActionItems SET MailToCustomer = 0 WHERE Item = ''Please explain recent changes in the directorship'''
	
	EXEC(@Statement)
END
GO

