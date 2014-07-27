IF OBJECT_ID('GetNonLimitedDataForCreditBureau') IS NULL
	EXECUTE('CREATE PROCEDURE GetNonLimitedDataForCreditBureau AS SELECT 1')
GO

ALTER PROCEDURE GetNonLimitedDataForCreditBureau
	(@RefNumber NVARCHAR(50))
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		Created,		
		RiskScore,
		Errors
	FROM 
		ExperianNonLimitedResults 
	WHERE 
		RefNumber = @RefNumber AND 
		IsActive = 1
END
GO
