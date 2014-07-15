IF OBJECT_ID('GetNonLimitedCompanyCreationTime') IS NULL
	EXECUTE('CREATE PROCEDURE GetNonLimitedCompanyCreationTime AS SELECT 1')
GO

ALTER PROCEDURE GetNonLimitedCompanyCreationTime
	(@CustomerId INT,		
	 @RefNumber NVARCHAR(50))
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT 
		Created 
	FROM 
		ExperianNonLimitedResults 
	WHERE 
		CustomerId = @CustomerId AND 
		RefNumber = @RefNumber AND 
		IsActive = 1
END
GO
