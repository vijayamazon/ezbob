IF OBJECT_ID('QuickOfferDataLoad') IS NULL
	EXECUTE('CREATE PROCEDURE QuickOfferDataLoad AS SELECT 1')
GO

ALTER PROCEDURE QuickOfferDataLoad
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CompanyRefNum NVARCHAR(50)
	DECLARE @DefaultCount INT
	DECLARE @AmlID BIGINT
	DECLARE @PersonalID BIGINT
	DECLARE @PersonalScore INT
	DECLARE @CompanyID BIGINT
	DECLARE @FirstName NVARCHAR(250)
	DECLARE @LastName NVARCHAR(250)
	DECLARE @RequestedAmount DECIMAL(18, 0)

	DECLARE @Enabled INT
	DECLARE @FundsAvailable DECIMAL(18, 4)
	DECLARE @LoanCount INT
	DECLARE @IssuedAmount DECIMAL(18, 4)
	DECLARE @OpenCashRequests DECIMAL(18, 4)
	DECLARE @ErrorMsg NVARCHAR(255)

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
		@ErrorMsg = ErrorMsg
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
		@LastName = cu.Surname
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
		(cu.ReferenceSource IS NULL OR cu.ReferenceSource != 'liqcen')

	---------------------------------------------------------------------------

	IF @CompanyRefNum IS NULL OR ISNULL(LTRIM(RTRIM(@CompanyRefNum)), '') = ''
	BEGIN
		EXECUTE xp_sprintf @FatalMsg OUTPUT, 'Customer %s - one of following: online customer/broker customer/fraud suspect/has no limited company/no company ref num in Company table.', @sCustomerID
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
		AND
		DATEDIFF(month, eda.Date, GETDATE()) < qoc.NoDefaultsInLastMonths

	---------------------------------------------------------------------------

	IF NOT ( @DefaultCount IS NULL OR @DefaultCount = 0 )
	BEGIN
		EXECUTE xp_sprintf @FatalMsg OUTPUT, 'Customer %s has default account(s).', @sCustomerID
		SELECT @FatalMsg AS FatalMsg
		RETURN
	END

	---------------------------------------------------------------------------
	--
	-- Check existence of AML score.
	--
	---------------------------------------------------------------------------

	SELECT TOP 1
		@AmlID = l.Id
	FROM
		MP_ServiceLog l
	WHERE
		l.CustomerId = @CustomerID
		AND
		l.ServiceType = 'AML A check'
	ORDER BY
		l.InsertDate DESC

	---------------------------------------------------------------------------

	IF @AmlID IS NULL
	BEGIN
		EXECUTE xp_sprintf @FatalMsg OUTPUT, 'No "AML A check" entry found in MP_ServiceLog for customer %s.', @sCustomerID
		SELECT @FatalMsg AS FatalMsg
		RETURN
	END

	---------------------------------------------------------------------------
	--
	-- Check existence of consumer data, age, and personal score.
	--
	---------------------------------------------------------------------------

	SELECT TOP 1
		@PersonalID = edc.Id,
		@PersonalScore = edc.ExperianScore
	FROM
		MP_ExperianDataCache edc
		INNER JOIN Customer c
			ON c.Id = @CustomerID
			AND edc.CustomerId = c.Id
			AND LOWER(LTRIM(RTRIM(edc.Name))) = LOWER(LTRIM(RTRIM(c.FirstName)))
			AND LOWER(LTRIM(RTRIM(edc.Surname))) = LOWER(LTRIM(RTRIM(c.Surname)))
		INNER JOIN QuickOfferConfiguration qoc ON qoc.ID = 1
	WHERE
		DATEDIFF(day, edc.BirthDate, GETDATE()) >= 365 * qoc.ApplicantMinAgeYears
		AND
		edc.ExperianScore >= qoc.PersonalScoreMin
	ORDER BY
		edc.LastUpdateDate DESC

	---------------------------------------------------------------------------

	IF @PersonalScore IS NULL
	BEGIN
		EXECUTE xp_sprintf @FatalMsg OUTPUT, 'Customer %s - one of following: no consumer data in MP_ExperianDataCache/too young/personal score is too low.', @sCustomerID
		SELECT @FatalMsg AS FatalMsg
		RETURN
	END

	---------------------------------------------------------------------------
	--
	-- Check existence of company data.
	--
	---------------------------------------------------------------------------

	SELECT TOP 1
		@CompanyID = edc.Id
	FROM
		MP_ExperianDataCache edc
	WHERE
		edc.CompanyRefNumber = @CompanyRefNum
	ORDER BY
		edc.LastUpdateDate DESC

	---------------------------------------------------------------------------

	IF @CompanyID IS NULL
	BEGIN
		EXECUTE xp_sprintf @FatalMsg OUTPUT, 'No company data found in MP_ExperianDataCache for company %s (customer %s).', @CompanyRefNum, @sCustomerID
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
		@AmlID AS AmlID,
		(SELECT ResponseData FROM MP_ServiceLog WHERE Id = @AmlID) AS AmlData,
		@PersonalID AS PersonalID,
		@PersonalScore AS PersonalScore,
		@CompanyID AS CompanyID,
		(SELECT JsonPacket FROM MP_ExperianDataCache WHERE Id = @CompanyID) AS CompanyData,
		LTRIM(RTRIM(@FirstName)) AS FirstName,
		LTRIM(RTRIM(@LastName)) AS LastName,
		(SELECT JsonPacket FROM MP_ExperianDataCache WHERE Id = @PersonalID) AS ConsumerData,
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
