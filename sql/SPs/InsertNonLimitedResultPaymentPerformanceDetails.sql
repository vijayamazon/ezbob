IF OBJECT_ID('InsertNonLimitedResultPaymentPerformanceDetails') IS NULL
	EXECUTE('CREATE PROCEDURE InsertNonLimitedResultPaymentPerformanceDetails AS SELECT 1')
GO

ALTER PROCEDURE InsertNonLimitedResultPaymentPerformanceDetails
	(@ExperianNonLimitedResultId INT,
	 @Code NVARCHAR(5),	
	 @DaysBeyondTerms INT)
AS
BEGIN
	SET NOCOUNT ON;
	
	INSERT INTO ExperianNonLimitedResultPerformanceDetails
		(ExperianNonLimitedResultId,
		 Code,	
		 DaysBeyondTerms)
	VALUES
		(@ExperianNonLimitedResultId,
		 @Code,	
		 @DaysBeyondTerms)
END
GO
