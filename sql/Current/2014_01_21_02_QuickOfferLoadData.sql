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
	
	SELECT
		@IsOffline = cu.IsOffline,
		@CompanyRefNum = co.ExperianRefNum
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
	
	IF @CompanyRefNum IS NOT NULL
	BEGIN
		SELECT
			@DefaultCount = COUNT(*)
		FROM
			ExperianDefaultAccount eda
		WHERE
			eda.CustomerId = @CustomerID
			AND
			DATEDIFF(day, eda.Date, GETDATE()) < 365 * 2
	END
	
	IF @DefaultCount = 0
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
		WHERE
			DATEDIFF(day, edc.BirthDate, GETDATE()) >= 365 * 18 + 4
		ORDER BY
			edc.LastUpdateDate DESC
	END
	
	IF @PersonalScore >= 560
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
		@IsOffline AS IsOffline,
		@CompanyRefNum AS CompanyRefNum,
		@DefaultCount AS DefaultCount,
		@AmlID AS AmlID,
		(SELECT ResponseData FROM MP_ServiceLog WHERE Id = @AmlID) AS AmlData,
		@PersonalID AS PersonalID,
		@PersonalScore AS PersonalScore,
		@CompanyID AS CompanyID,
		(SELECT JsonPacket FROM MP_ExperianDataCache WHERE Id = @CompanyID) AS CompanyData
	WHERE
		@CompanyRefNum IS NOT NULL
END
GO
