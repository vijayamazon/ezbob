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

	SET @IsBeingProcessed = ISNULL(@IsBeingProcessed, 1)

	UPDATE Customer SET
		CreditResult = CASE @IsBeingProcessed WHEN 1 THEN NULL   ELSE 'WaitingForDecision' END,
		Status       = CASE @IsBeingProcessed WHEN 1 THEN Status ELSE 'Manual'             END
	WHERE
		Id = @CustomerID
END
GO
