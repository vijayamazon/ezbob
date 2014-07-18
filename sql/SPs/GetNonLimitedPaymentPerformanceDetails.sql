IF OBJECT_ID('GetNonLimitedPaymentPerformanceDetails') IS NULL
	EXECUTE('CREATE PROCEDURE GetNonLimitedPaymentPerformanceDetails AS SELECT 1')
GO

ALTER PROCEDURE GetNonLimitedPaymentPerformanceDetails
	(@ExperianNonLimitedResultId INT)
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		Code,
		DaysBeyondTerms
	FROM 
		ExperianNonLimitedResultPaymentPerformanceDetails
	WHERE 
		ExperianNonLimitedResultId = @ExperianNonLimitedResultId
END
GO
