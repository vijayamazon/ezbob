IF OBJECT_ID('GetNonLimitedPreviousSearches') IS NULL
	EXECUTE('CREATE PROCEDURE GetNonLimitedPreviousSearches AS SELECT 1')
GO

ALTER PROCEDURE GetNonLimitedPreviousSearches
	(@ExperianNonLimitedResultId INT)
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		PreviousSearchDate,
		EnquiryType,	
		EnquiryTypeDesc,
		CreditRequired
	FROM 
		ExperianNonLimitedResultPreviousSearches
	WHERE 
		ExperianNonLimitedResultId = @ExperianNonLimitedResultId
END
GO
