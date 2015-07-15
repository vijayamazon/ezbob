IF OBJECT_ID('ValidateCustomerAndCashRequest') IS NULL
	EXECUTE('CREATE PROCEDURE ValidateCustomerAndCashRequest AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE ValidateCustomerAndCashRequest
@CustomerID INT,
@CashRequestID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @IsMatch BIT = 0

	IF EXISTS (SELECT * FROM CashRequests WHERE Id = @CashRequestID AND IdCustomer = @CustomerID)
		SET @IsMatch = 1

	SELECT
		IsMatch = @IsMatch,
		CustomerID = @CustomerID,
		CashRequestID = @CashRequestID
END
GO
