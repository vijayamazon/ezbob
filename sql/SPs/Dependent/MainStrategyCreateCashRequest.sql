SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('MainStrategyCreateCashRequest') IS NULL
	EXECUTE('CREATE PROCEDURE MainStrategyCreateCashRequest AS SELECT 1')
GO


ALTER PROCEDURE MainStrategyCreateCashRequest
@CustomerID INT,
@Now DATETIME,
@Originator NVARCHAR(30)
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @CashRequestID BIGINT
	DECLARE @LastLoanAmount DECIMAL(18, 0)
	DECLARE @CashRequestCount INT

	------------------------------------------------------------------------------

	DECLARE @LsID INT
	DECLARE @LsMaxInterestRate DECIMAL(18, 6)
	DECLARE @LsDefaultRepaymentPeriod INT
	DECLARE @LsIsCustomerRepaymentPeriodSelectionAllowed BIT

	DECLARE @IsAlibaba BIT
	DECLARE @OriginID INT
	DECLARE @LtType NVARCHAR(50)
	DECLARE @RepaymentPeriod INT

	DECLARE @LtID INT
	DECLARE @LtRepaymentPeriod INT

	------------------------------------------------------------------------------

	SELECT
		@IsAlibaba = IsAlibaba,
		@OriginID = OriginID
	FROM
		Customer
	WHERE
		Id = @CustomerID

	SELECT @IsAlibaba = ISNULL(@IsAlibaba, 0)

	------------------------------------------------------------------------------

	SET @LtType = CASE WHEN @IsAlibaba = 1 THEN 'AlibabaLoanType' ELSE NULL END

	------------------------------------------------------------------------------

	SELECT
		@LtID = LoanTypeID,
		@LtRepaymentPeriod = RepaymentPeriod
	FROM
		dbo.udfGetLoanTypeByType(@LtType)

	------------------------------------------------------------------------------

	SELECT
		@LsID = LoanSourceID,
		@LsMaxInterestRate = MaxInterest,
		@LsDefaultRepaymentPeriod = DefaultRepaymentPeriod,
		@LsIsCustomerRepaymentPeriodSelectionAllowed = IsCustomerRepaymentPeriodSelectionAllowed
	FROM
		dbo.udfGetLoanSource(NULL, @OriginID)

	------------------------------------------------------------------------------

	SET @RepaymentPeriod = ISNULL(ISNULL(@LsDefaultRepaymentPeriod, @LtRepaymentPeriod), 12)

	------------------------------------------------------------------------------

	INSERT INTO CashRequests (
		IdCustomer, CreationDate, InterestRate,
		UseSetupFee, LoanTypeId,
		RepaymentPeriod, ApprovedRepaymentPeriod,
		LoanSourceID, IsCustomerRepaymentPeriodSelectionAllowed,
		Originator
	) VALUES (
		@CustomerID, @Now, ISNULL(@LsMaxInterestRate, 0.0225),
		0, @LtID,
		@RepaymentPeriod, @RepaymentPeriod,
		@LsID, @LsIsCustomerRepaymentPeriodSelectionAllowed,
		@Originator
	)

	------------------------------------------------------------------------------

	SET @CashRequestID = SCOPE_IDENTITY()

	------------------------------------------------------------------------------

	EXECUTE MainStrategySetCustomerIsBeingProcessed @CustomerID

	------------------------------------------------------------------------------

	SELECT TOP 1
		@LastLoanAmount = Amount
	FROM
		CustomerRequestedLoan
	WHERE
		CustomerId = @CustomerID
	ORDER BY
		Id DESC

	------------------------------------------------------------------------------

	SET @CashRequestCount = ISNULL((
		SELECT
			COUNT(*)
		FROM
			CashRequests
		WHERE
			IdCustomer = @CustomerID
	), 0)

	------------------------------------------------------------------------------

	SELECT
		CashRequestID = @CashRequestID,
		LastLoanAmount = @LastLoanAmount,
		CashRequestCount = @CashRequestCount
END
GO
