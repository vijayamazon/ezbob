IF OBJECT_ID('GetNonLimitedCompanyCreationTime') IS NULL
	EXECUTE('CREATE PROCEDURE GetNonLimitedCompanyCreationTime AS SELECT 1')
GO

ALTER PROCEDURE GetNonLimitedCompanyCreationTime
	(@RefNumber NVARCHAR(50))
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT 
		Created 
	FROM 
		ExperianNonLimitedResults 
	WHERE 
		RefNumber = @RefNumber AND 
		IsActive = 1
END
GO
