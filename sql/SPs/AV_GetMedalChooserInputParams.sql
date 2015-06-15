SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_GetMedalChooserInputParams') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetMedalChooserInputParams AS SELECT 1')
GO

ALTER PROCEDURE AV_GetMedalChooserInputParams
@CustomerId INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @eBay   UNIQUEIDENTIFIER = 'A7120CB7-4C93-459B-9901-0E95E7281B59'
	DECLARE @Amazon UNIQUEIDENTIFIER = 'A4920125-411F-4BB9-A52D-27E8A00D0A3B'
	DECLARE @PayPal UNIQUEIDENTIFIER = '3FA5E327-FCFD-483B-BA5A-DC1815747A28'
	DECLARE @Yodlee UNIQUEIDENTIFIER = '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF'
	DECLARE @HMRC   UNIQUEIDENTIFIER = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'

	DECLARE @IsLimited BIT = 0
	DECLARE @HasOnline BIT = 0
	DECLARE @HasHmrc BIT = 0
	DECLARE @HasBank BIT = 0
	DECLARE @HasCompanyScore BIT = 0
	DECLARE @HasPersonalScore BIT = 0
	DECLARE @LastHmrcUpdateDate DATETIME
	DECLARE @LastBankUpdateDate DATETIME
	DECLARE @TypeOfBusiness NVARCHAR(30)
	DECLARE @CompanyRefNum NVARCHAR(30)
	DECLARE @PersonalScore INT = 0
	
	----------------------------------------------------------------------------
	SELECT
		@TypeOfBusiness = c.TypeOfBusiness,
		@CompanyRefNum = co.ExperianRefNum
	FROM
		Customer c
		LEFT JOIN Company co ON c.CompanyId = co.Id
	WHERE
		c.Id = @CustomerId
		
		
	----------------------------------------------------------------------------
	DECLARE @ServiceLogId BIGINT
	EXEC GetExperianConsumerServiceLog @CustomerID, @ServiceLogId OUTPUT, @Now
	
	-- Minimal Consumer/Directors score
	
	DECLARE @ExperianConsumerDataID BIGINT
	
	SELECT
		@ExperianConsumerDataID = e.Id
	FROM
		ExperianConsumerData e
	WHERE
		e.ServiceLogId = @ServiceLogId
	
	----------------------------------------------------------------------------
	
	SELECT
		@PersonalScore = MIN(x.ExperianConsumerScore)
	FROM	(
		SELECT ISNULL(d.BureauScore, 0) AS ExperianConsumerScore
		FROM ExperianConsumerData d
		INNER JOIN MP_ServiceLog l ON d.ServiceLogId = l.Id
		WHERE d.Id = @ExperianConsumerDataID
	
		UNION
	
		SELECT ISNULL(d.MinScore, 0) AS ExperianConsumerScore
		FROM CustomerAnalyticsDirector d
		WHERE d.CustomerID = @CustomerId
		AND d.IsActive = 1
	) x
	----------------------------------------------------------------------------

	IF @PersonalScore > 0
		SET @HasPersonalScore = 1
	----------------------------------------------------------------------------
	IF @TypeOfBusiness = 'Limited' OR @TypeOfBusiness = 'LLP'
		SET @IsLimited = 1
	----------------------------------------------------------------------------
	DECLARE @CompanyScore INT = 0	

	IF @IsLimited = 1
	BEGIN
		SELECT TOP 1
			@CompanyScore = cac.Score
		FROM
			CustomerAnalyticsCompany cac
		WHERE
			cac.CustomerID = @CustomerId
			AND
			cac.AnalyticsDate < @Now
		ORDER BY
			cac.AnalyticsDate DESC

		IF @CompanyScore > 0 
			SET @HasCompanyScore = 1
	END
	ELSE BEGIN 
		SELECT TOP 1
			@CompanyScore = nl.CommercialDelphiScore
		FROM
			ExperianNonLimitedResults nl
			INNER JOIN MP_ServiceLog l ON nl.ServiceLogId = l.Id
		WHERE
			nl.RefNumber = @CompanyRefNum
			AND
			l.InsertDate < @Now
		ORDER BY
			l.InsertDate DESC

		IF @CompanyScore > 0
			SET @HasCompanyScore = 1
	END
	----------------------------------------------------------------------------
	IF EXISTS (
		SELECT *
		FROM MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t ON t.Id = m.MarketPlaceId
		WHERE m.CustomerId = @CustomerId
		AND m.Disabled = 0
		AND t.InternalId IN (@eBay, @Amazon, @PayPal)
		AND m.Created < @Now
	)
	BEGIN
		SET @HasOnline = 1
	END

	IF EXISTS (
		SELECT *
		FROM MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t ON t.Id = m.MarketPlaceId
		WHERE m.CustomerId = @CustomerId
		AND m.Disabled = 0
		AND t.InternalId = @Yodlee
		AND m.Created < @Now
	)
	BEGIN
		SET @HasBank = 1
	END
	----------------------------------------------------------------------------
	IF EXISTS (
		SELECT *
		FROM MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t ON t.Id = m.MarketPlaceId
		INNER JOIN MP_VatReturnRecords r ON m.Id = r.CustomerMarketPlaceId
		INNER JOIN Business b ON r.BusinessId = b.Id AND b.BelongsToCustomer = 1
		WHERE m.CustomerId = @CustomerId
		AND m.Disabled = 0
		AND t.InternalId = @HMRC
		AND m.Created < @Now
	)
	BEGIN
		SET @HasHmrc = 1
	END
	----------------------------------------------------------------------------
	SELECT
		@LastHmrcUpdateDate = MIN(m.UpdatingEnd)
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t ON t.Id = m.MarketPlaceId
		INNER JOIN MP_VatReturnRecords r ON m.Id = r.CustomerMarketPlaceId
		INNER JOIN Business b
			ON r.BusinessId = b.Id
			AND b.BelongsToCustomer = 1
	WHERE
		m.CustomerId = @CustomerId
		AND
		m.Disabled = 0
		AND
		t.InternalId = @HMRC
		AND
		m.Created < @Now
		AND
		m.UpdatingEnd IS NOT NULL
	----------------------------------------------------------------------------
	SELECT
		@LastBankUpdateDate = MIN(m.UpdatingEnd)
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t ON t.Id = m.MarketPlaceId
	WHERE
		m.CustomerId = @CustomerId
		AND
		m.Disabled = 0
		AND
		t.InternalId = @Yodlee
		AND
		m.UpdatingEnd IS NOT NULL
		AND
		m.Created < @Now
	----------------------------------------------------------------------------
	DECLARE @MinApprovalAmount INT = (
		SELECT CAST(Value AS INT)
		FROM ConfigurationVariables
		WHERE Name = 'MedalMinOffer'
	)
	----------------------------------------------------------------------------
	DECLARE @MedalDaysOfMpRelevancy INT = (
		SELECT CAST(Value AS INT)
		FROM ConfigurationVariables
		WHERE Name = 'MedalDaysOfMpRelevancy'
	)
	----------------------------------------------------------------------------
	SELECT
		@IsLimited AS IsLimited,
		@HasOnline AS HasOnline,
		@HasHmrc AS HasHmrc,
		@HasBank AS HasBank,
		@HasCompanyScore AS HasCompanyScore,
		@HasPersonalScore AS HasPersonalScore,
		@LastHmrcUpdateDate AS LastHmrcUpdateDate,
		@LastBankUpdateDate AS LastBankUpdateDate,
		@MedalDaysOfMpRelevancy AS MedalDaysOfMpRelevancy,
		@MinApprovalAmount AS MinApprovalAmount
END 
GO
