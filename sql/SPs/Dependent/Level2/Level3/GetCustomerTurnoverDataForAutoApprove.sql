IF OBJECT_ID('GetCustomerTurnoverDataForAutoApprove') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerTurnoverDataForAutoApprove AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomerTurnoverDataForAutoApprove
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	EXECUTE GetCustomerTurnoverData 1, @CustomerID,  1, @Now
	EXECUTE GetCustomerTurnoverData 1, @CustomerID,  3, @Now
	EXECUTE GetCustomerTurnoverData 1, @CustomerID,  6, @Now
	EXECUTE GetCustomerTurnoverData 1, @CustomerID, 12, @Now
END
GO
