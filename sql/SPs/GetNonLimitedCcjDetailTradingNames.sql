IF OBJECT_ID('GetNonLimitedCcjDetailTradingNames') IS NULL
	EXECUTE('CREATE PROCEDURE GetNonLimitedCcjDetailTradingNames AS SELECT 1')
GO

ALTER PROCEDURE GetNonLimitedCcjDetailTradingNames
	(@ExperianNonLimitedResultCcjDetailsId INT)
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		Name,
		TradingIndicator,
		TradingIndicatorDesc
	FROM 
		ExperianNonLimitedResultCcjTradingNames
	WHERE 
		ExperianNonLimitedResultCcjDetailsId = @ExperianNonLimitedResultCcjDetailsId
END
GO
