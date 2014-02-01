IF OBJECT_ID('QuickOfferDataLoad') IS NULL
	EXECUTE('CREATE PROCEDURE QuickOfferDataLoad AS SELECT 1')
GO

ALTER PROCEDURE QuickOfferDataLoad
@CustomerID INT
AS
BEGIN
	DECLARE @IsOffline BIT
	DECLARE @CompanyRefNum NVARCHAR(50)
	DECLARE @DefaultCount INT
	DECLARE @AmlID BIGINT
	DECLARE @PersonalID BIGINT
	DECLARE @PersonalScore INT
	DECLARE @CompanyID BIGINT
	DECLARE @FirstName NVARCHAR(250)
	DECLARE @LastName NVARCHAR(250)
	DECLARE @RequestedAmount DECIMAL(18, 0)

	SELECT TOP 1
		@RequestedAmount = Amount
	FROM
		CustomerRequestedLoan
	WHERE
		CustomerId = @CustomerId
	ORDER BY
		Created DESC

	IF @RequestedAmount IS NULL
		SELECT @RequestedAmount = 0

	IF @RequestedAmount > 0
	BEGIN
		SELECT
			@IsOffline = cu.IsOffline,
			@CompanyRefNum = co.ExperianRefNum,
			@FirstName = cu.FirstName,
			@LastName = cu.Surname
		FROM
			Customer cu
			INNER JOIN Company co
				ON cu.Id = co.CustomerId
				AND co.TypeOfBusiness IN ('Limited', 'LLP')
				AND co.ExperianRefNum IS NOT NULL
				AND co.ExperianRefNum != ''
		WHERE
			cu.Id = @CustomerID
			AND
			(cu.ReferenceSource IS NULL OR cu.ReferenceSource != 'liqcen')
			AND
			cu.FraudStatus = 0
	END
	
	IF @CompanyRefNum IS NOT NULL
	BEGIN
		SELECT
			@DefaultCount = COUNT(*)
		FROM
			ExperianDefaultAccount eda
			INNER JOIN QuickOfferConfiguration qoc ON qoc.ID = 1
		WHERE
			eda.CustomerId = @CustomerID
			AND
			DATEDIFF(month, eda.Date, GETDATE()) < qoc.NoDefaultsInLastMonths
	END
	
	IF @DefaultCount IS NULL OR @DefaultCount = 0
	BEGIN
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
	END
	
	IF @AmlID IS NOT NULL
	BEGIN
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
	END
	
	IF @PersonalScore IS NOT NULL
	BEGIN
		SELECT TOP 1
			@CompanyID = edc.Id
		FROM
			MP_ExperianDataCache edc
		WHERE
			edc.CompanyRefNumber = @CompanyRefNum
		ORDER BY
			edc.LastUpdateDate DESC
	END

	SELECT
		@CustomerID AS CustomerID,
		@RequestedAmount AS RequestedAmount,
		@IsOffline AS IsOffline,
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
		(SELECT JsonPacket FROM MP_ExperianDataCache WHERE Id = @PersonalID) AS ConsumerData
END
GO
