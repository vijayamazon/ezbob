IF OBJECT_ID('MainStrategySetCustomerIsBeingProcessed') IS NULL
	EXECUTE('CREATE PROCEDURE MainStrategySetCustomerIsBeingProcessed AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE MainStrategySetCustomerIsBeingProcessed
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Customer SET
		CreditResult = NULL
	WHERE
		Id = @CustomerID
END
GO
