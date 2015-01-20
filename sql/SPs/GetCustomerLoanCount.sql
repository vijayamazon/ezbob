IF OBJECT_ID('GetCustomerLoanCount') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerLoanCount AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomerLoanCount
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Counter INT

	SELECT
		@Counter = COUNT(1)
	FROM
		Loan
	WHERE
		CustomerID = @CustomerID
		AND
		[Date] < @Now

	SELECT
		LoanCount = ISNULL(@Counter, 0)
END
GO
