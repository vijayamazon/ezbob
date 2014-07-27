IF OBJECT_ID('GetNonLimitedDataForLoanDateScoreReport') IS NULL
	EXECUTE('CREATE PROCEDURE GetNonLimitedDataForLoanDateScoreReport AS SELECT 1')
GO

ALTER PROCEDURE GetNonLimitedDataForLoanDateScoreReport
	(@RefNumber NVARCHAR(50))
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
		RefNumber = @RefNumber AND 
		IsActive = 1
END
GO
