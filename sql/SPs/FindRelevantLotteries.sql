SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('FindRelevantLotteries') IS NULL
	EXECUTE('CREATE PROCEDURE FindRelevantLotteries AS SELECT 1')
GO

ALTER PROCEDURE FindRelevantLotteries
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @UserID INT
	DECLARE @BrokerID INT
	DECLARE @LoanCount INT
	DECLARE @LoanAmount DECIMAL(18, 2)

	SELECT
		@UserID = c.Id,
		@BrokerID = c.BrokerID
	FROM
		Customer c
	WHERE
		c.Id = @CustomerID

	IF @UserID != @CustomerID -- wrong customer ID specified
		RETURN

	SET @BrokerID = dbo.udfMaxInt(ISNULL(@BrokerID, 0), 0)

	IF @BrokerID = 0
	BEGIN
		SELECT
			@LoanCount = COUNT(*),
			@LoanAmount = SUM(l.LoanAmount)
		FROM
			Loan l
			INNER JOIN CashRequests r ON l.RequestCashId = r.Id
		WHERE
			r.IdCustomer = @CustomerID
	END
	ELSE BEGIN
		SELECT
			@LoanCount = COUNT(*),
			@LoanAmount = SUM(l.LoanAmount)
		FROM
			Loan l
			INNER JOIN CashRequests r ON l.RequestCashId = r.Id
			INNER JOIN Customer c ON r.IdCustomer = c.Id
		WHERE
			c.BrokerID = @BrokerID
	END

	SELECT
		RowType = 'MetaData',
		BrokerID = @BrokerID,
		LoanCount = ISNULL(@LoanCount, 0),
		LoanAmount = ISNULL(@LoanAmount, 0)

	DECLARE @IsForCustomer BIT = CASE WHEN @BrokerID = 0 THEN 1 ELSE 0 END

	SELECT
		RowType = 'Lottery',
		l.LotteryID,
		l.LoanCount,
		l.LoanAmount,
		l.LotteryEnlistingTypeID,
		et.LotteryEnlistingType
	FROM
		Lotteries l
		INNER JOIN LotteryEnlistingTypes et ON l.LotteryEnlistingTypeID = et.LotteryEnlistingTypeID
	WHERE
		l.StartDate <= @Now AND @Now <= l.EndDate
		AND
		l.IsActive = 1
		AND
		l.IsForCustomer = @IsForCustomer
END
GO
