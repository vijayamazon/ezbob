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
	DECLARE @CompanyServiceLogID BIGINT
	DECLARE @PersonalScore INT = 0

	----------------------------------------------------------------------------

	SELECT
		@CompanyRefNum = CompanyRefNum,
		@CompanyServiceLogID = ServiceLogID,
		@TypeOfBusiness = TypeOfBusiness
	FROM
		dbo.udfGetCustomerHistoricalCompanyLogID(@CustomerID, @Now)

	----------------------------------------------------------------------------

	SELECT
		@PersonalScore = MIN(x.ExperianConsumerScore)
	FROM	(
		SELECT
			ISNULL(d.BureauScore, 0) AS ExperianConsumerScore
		FROM
			ExperianConsumerData d
			INNER JOIN dbo.udfLoadCustomerAndDirectorsServiceLog(@CustomerId, @Now) l
				ON d.Id = l.ExperianConsumerDataID
	) x

	----------------------------------------------------------------------------

	IF @PersonalScore > 0
		SET @HasPersonalScore = 1

	----------------------------------------------------------------------------
	
	IF @TypeOfBusiness IN ('Limited', 'LLP')
		SET @IsLimited = 1

	----------------------------------------------------------------------------

	DECLARE @CompanyScore INT = 0	

	IF @IsLimited = 1
	BEGIN
		SELECT
			@CompanyScore = ISNULL(CommercialDelphiScore, 0)
		FROM
			ExperianLtd
		WHERE
			ServiceLogID = @CompanyServiceLogID
	END
	ELSE BEGIN 
		SELECT
			@CompanyScore = nl.CommercialDelphiScore
		FROM
			ExperianNonLimitedResults nl
		WHERE
			nl.ServiceLogID = @CompanyServiceLogID
	END

	IF @CompanyScore > 0
		SET @HasCompanyScore = 1

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

	----------------------------------------------------------------------------

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
