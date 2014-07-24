IF OBJECT_ID('InsertNonLimitedResultCcjTradingNames') IS NULL
	EXECUTE('CREATE PROCEDURE InsertNonLimitedResultCcjTradingNames AS SELECT 1')
GO

ALTER PROCEDURE InsertNonLimitedResultCcjTradingNames
	(@ExperianNonLimitedResultCcjDetailsId INT,
	 @Name NVARCHAR(75),
	 @TradingIndicator NVARCHAR(1),		
	 @TradingIndicatorDesc NVARCHAR(25))
AS
BEGIN
	SET NOCOUNT ON;
	
	INSERT INTO ExperianNonLimitedResultCcjTradingNames
		(ExperianNonLimitedResultCcjDetailsId, Name, TradingIndicator, TradingIndicatorDesc)
	VALUES
		(@ExperianNonLimitedResultCcjDetailsId, @Name, @TradingIndicator, @TradingIndicatorDesc)
END
GO
