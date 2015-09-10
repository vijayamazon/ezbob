IF OBJECT_ID('MainStrategySetCustomerIsBeingProcessed') IS NULL
	EXECUTE('CREATE PROCEDURE MainStrategySetCustomerIsBeingProcessed AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE MainStrategySetCustomerIsBeingProcessed
@CustomerID INT,
@IsBeingProcessed BIT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Customer SET
		CreditResult = CASE ISNULL(@IsBeingProcessed, 1) WHEN 1 THEN NULL ELSE 'WaitingForDecision' END
	WHERE
		Id = @CustomerID
END
GO
