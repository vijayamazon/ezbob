IF OBJECT_ID('dbo.udfLoanTransactionTimeStatus') IS NOT NULL
	DROP FUNCTION dbo.udfLoanTransactionTimeStatus
GO

CREATE FUNCTION dbo.udfLoanTransactionTimeStatus (
	@TransactionID INT
)
RETURNS NVARCHAR(6)
AS
BEGIN
	DECLARE @stat TABLE (
		Status NVARCHAR(50) NOT NULL
	)

	INSERT INTO @stat
	SELECT DISTINCT
		StatusBefore
	FROM
		LoanScheduleTransaction
	WHERE
		TransactionID = @TransactionID
	UNION
	SELECT DISTINCT
		StatusAfter
	FROM
		LoanScheduleTransaction
	WHERE
		TransactionID = @TransactionID

	IF EXISTS (SELECT * FROM @stat WHERE Status IN ('Late', 'Paid'))
		RETURN 'Late'

	IF EXISTS (SELECT * FROM @stat WHERE Status IN ('PaidEarly'))
		RETURN 'Early'

	RETURN 'OnTime'
END
GO
