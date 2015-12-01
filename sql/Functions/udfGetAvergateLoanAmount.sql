SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.udfGetAvergateLoanAmount') IS NOT NULL
	DROP FUNCTION dbo.udfGetAvergateLoanAmount
GO

CREATE FUNCTION dbo.udfGetAvergateLoanAmount()
RETURNS INT
AS
BEGIN
	DECLARE @LoanAmount DECIMAL(18, 4)
	DECLARE @TakenCount INT

	;WITH stat AS (
		SELECT
			LoanAmount = l.LoanAmount,
			TakenCount = COUNT(*)
		FROM
			Loan l
			INNER JOIN CashRequests r ON l.RequestCashId = r.Id
			INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
		GROUP BY
			l.LoanAmount
	)
	SELECT
		@LoanAmount = SUM(LoanAmount * TakenCount),
		@TakenCount = SUM(TakenCount)
	FROM
		stat

	IF @TakenCount = 0
		RETURN 0

	RETURN CONVERT(INT, @LoanAmount / @TakenCount)
END
GO
