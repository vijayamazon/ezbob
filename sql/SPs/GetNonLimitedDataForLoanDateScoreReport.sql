IF OBJECT_ID('GetNonLimitedDataForLoanDateScoreReport') IS NULL
	EXECUTE('CREATE PROCEDURE GetNonLimitedDataForLoanDateScoreReport AS SELECT 1')
GO

ALTER PROCEDURE GetNonLimitedDataForLoanDateScoreReport
	(@CustomerId INT,		
	 @RefNumber NVARCHAR(50))
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		CommercialDelphiScore, 
		ProbabilityOfDefaultScore, 
		StabilityOdds
	FROM 
		ExperianNonLimitedResults 
	WHERE 
		CustomerId = @CustomerId AND 
		RefNumber = @RefNumber AND 
		IsActive = 1
END
GO
