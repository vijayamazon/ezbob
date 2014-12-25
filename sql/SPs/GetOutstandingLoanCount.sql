IF OBJECT_ID('GetOutstandingLoanCount') IS NULL
	EXECUTE('CREATE PROCEDURE GetOutstandingLoanCount AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetOutstandingLoanCount
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @n INT

	SELECT
		@n = COUNT(*)
	FROM
		Loan
	WHERE
		CustomerId = @CustomerID
		AND
		Status != 'PaidOff'

	SELECT LoanCount = ISNULL(@n, 0)
END
GO
