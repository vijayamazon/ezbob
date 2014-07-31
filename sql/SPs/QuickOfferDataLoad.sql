IF OBJECT_ID('QuickOfferDataLoad') IS NULL
	EXECUTE('CREATE PROCEDURE QuickOfferDataLoad AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE QuickOfferDataLoad
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CompanyRefNum NVARCHAR(50)
	DECLARE @DefaultCount INT
	DECLARE @AmlScore INT
	DECLARE @FirstName NVARCHAR(250)
	DECLARE @LastName NVARCHAR(250)
	DECLARE @RequestedAmount DECIMAL(18, 0)
	DECLARE @ConsumerScore INT

	DECLARE @Enabled INT
	DECLARE @FundsAvailable DECIMAL(18, 4)
	DECLARE @LoanCount INT
	DECLARE @IssuedAmount DECIMAL(18, 4)
	DECLARE @OpenCashRequests DECIMAL(18, 4)
	DECLARE @ErrorMsg NVARCHAR(255)
	DECLARE @AmlMin INT
	DECLARE @ConsumerScoreMin INT
	DECLARE @ApplicantMinAgeYears INT

	DECLARE @FatalMsg NVARCHAR(255)
	
	DECLARE @sCustomerID NVARCHAR(24)
	SET @sCustomerID = CONVERT(NVARCHAR, @CustomerID)

	---------------------------------------------------------------------------
	--
	-- Check configuration
	--
	---------------------------------------------------------------------------

	SELECT
		@Enabled = Enabled,
		@FundsAvailable = FundsAvailable,
		@LoanCount = LoanCount,
		@IssuedAmount = IssuedAmount,
		@OpenCashRequests = OpenCashRequests,
		@ErrorMsg = ErrorMsg,
		@AmlMin = AmlMin,
		@ConsumerScoreMin = ConsumerScoreMin,
		@ApplicantMinAgeYears = ApplicantMinAgeYears
	FROM
		dbo.udfCanQuickOffer()

	---------------------------------------------------------------------------

	IF @Enabled = 0 OR @ErrorMsg != ''
	BEGIN
		EXECUTE xp_sprintf @FatalMsg OUTPUT, 'Quick offer protection error for customer %s: %s', @sCustomerID, @ErrorMsg
		SELECT @FatalMsg AS FatalMsg
		RETURN
	END

	---------------------------------------------------------------------------
	--
	-- Check requested amount
	--
	---------------------------------------------------------------------------

	SELECT TOP 1
		@RequestedAmount = Amount
	FROM
		CustomerRequestedLoan
	WHERE
		CustomerId = @CustomerId
	ORDER BY
		Created DESC

	---------------------------------------------------------------------------

	IF @RequestedAmount IS NULL OR @RequestedAmount <= 0
	BEGIN
		EXECUTE xp_sprintf @FatalMsg OUTPUT, 'Requested amount was not found for customer %s.', @sCustomerID
		SELECT @FatalMsg AS FatalMsg
		RETURN
	END

	---------------------------------------------------------------------------
	--
	-- Check company type, broker customer, offline customer, fraud status,
	-- and existence of company data.
	--
	---------------------------------------------------------------------------

	SELECT
		@CompanyRefNum = co.ExperianRefNum,
		@FirstName = cu.FirstName,
		@LastName = cu.Surname,
		@AmlScore = cu.AmlScore,
		@ConsumerScore = cu.ExperianConsumerScore
	FROM
		Customer cu
		INNER JOIN Company co
			ON cu.CompanyId = co.Id
			AND co.TypeOfBusiness IN ('Limited', 'LLP')
			AND co.ExperianRefNum IS NOT NULL
			AND co.ExperianRefNum != ''
	WHERE
		cu.Id = @CustomerID
		AND
		cu.FraudStatus = 0
		AND
		cu.IsOffline = 1
		AND
		cu.BrokerID IS NULL
		AND
		(cu.ReferenceSource IS NULL OR cu.ReferenceSource != 'liqcen')
		AND
		DATEDIFF(day, cu.DateOfBirth, GETDATE()) >= 365 * @ApplicantMinAgeYears

	---------------------------------------------------------------------------

	IF @CompanyRefNum IS NULL OR ISNULL(LTRIM(RTRIM(@CompanyRefNum)), '') = ''
	BEGIN
		EXECUTE xp_sprintf @FatalMsg OUTPUT, 'Customer %s - one of following: online customer/too young/broker customer/fraud suspect/has no limited company/no company ref num in Company table.', @sCustomerID
		SELECT @FatalMsg AS FatalMsg
		RETURN
	END

	---------------------------------------------------------------------------

	IF @AmlScore IS NULL OR @AmlScore < @AmlMin
	BEGIN
		EXECUTE xp_sprintf @FatalMsg OUTPUT, 'AML score is too low for customer %s.', @sCustomerID
		SELECT @FatalMsg AS FatalMsg
		RETURN
	END

	---------------------------------------------------------------------------

	IF @ConsumerScore IS NULL OR @ConsumerScore < @ConsumerScoreMin
	BEGIN
		EXECUTE xp_sprintf @FatalMsg OUTPUT, 'Customer %s: personal score is too low.', @sCustomerID
		SELECT @FatalMsg AS FatalMsg
		RETURN
	END

	---------------------------------------------------------------------------
	--
	-- Check default count.
	--
	---------------------------------------------------------------------------

	SELECT
		@DefaultCount = COUNT(*)
	FROM
		ExperianDefaultAccount eda
		INNER JOIN QuickOfferConfiguration qoc ON qoc.ID = 1
	WHERE
		eda.CustomerId = @CustomerID
		AND (
			DATEDIFF(month, eda.Date, GETDATE()) < qoc.NoDefaultsInLastMonths -- default in last X months
			OR
			eda.Balance > 0 -- unsettled default
		)

	---------------------------------------------------------------------------

	IF NOT ( @DefaultCount IS NULL OR @DefaultCount = 0 )
	BEGIN
		EXECUTE xp_sprintf @FatalMsg OUTPUT, 'Customer %s has default account(s).', @sCustomerID
		SELECT @FatalMsg AS FatalMsg
		RETURN
	END

	---------------------------------------------------------------------------
	--
	-- Final output.
	--
	---------------------------------------------------------------------------

	SELECT
		@CustomerID AS CustomerID,
		@RequestedAmount AS RequestedAmount,
		@CompanyRefNum AS CompanyRefNum,
		@DefaultCount AS DefaultCount,
		@AmlScore AS AmlScore,
		LTRIM(RTRIM(@FirstName)) AS FirstName,
		LTRIM(RTRIM(@LastName)) AS LastName,
		@ConsumerScore AS ConsumerScore,
		@Enabled AS Enabled,
		@FundsAvailable AS FundsAvailable,
		@LoanCount AS LoanCount,
		@IssuedAmount AS IssuedAmount,
		@OpenCashRequests AS OpenCashRequests,
		@ErrorMsg AS ErrorMsg,
		'' AS FatalMsg

	---------------------------------------------------------------------------
	--
	-- That's all, folks!
	--
	---------------------------------------------------------------------------
END
GO
