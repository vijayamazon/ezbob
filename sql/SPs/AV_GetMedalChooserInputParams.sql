SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_GetMedalChooserInputParams') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetMedalChooserInputParams AS SELECT 1')
GO

ALTER PROCEDURE AV_GetMedalChooserInputParams
@CustomerId INT
AS
BEGIN

DECLARE @IsLimited BIT = 0
DECLARE @HasOnline BIT = 0
DECLARE @HasHmrc BIT = 0
DECLARE @HasBank BIT = 0
DECLARE @HasCompanyScore BIT = 0
DECLARE @HasPersonalScore BIT = 0
DECLARE @NumOfHmrc INT = 0
DECLARE @LastBankHmrcUpdateDate DATETIME  

DECLARE @TypeOfBusiness NVARCHAR(30)
DECLARE @CompanyRefNum NVARCHAR(30)
DECLARE @PersonalScore INT = 0
SELECT @TypeOfBusiness = c.TypeOfBusiness, @CompanyRefNum = co.ExperianRefNum, @PersonalScore = c.ExperianConsumerScore
FROM Customer c LEFT JOIN Company co ON c.CompanyId=co.Id 
WHERE c.Id=@CustomerId

IF @PersonalScore > 0
	SET @HasPersonalScore = 1


IF @TypeOfBusiness = 'Limited' OR @TypeOfBusiness = 'LLP' 
	SET @IsLimited = 1

DECLARE @CompanyScore INT = 0	
IF @IsLimited = 1
BEGIN
	SELECT @CompanyScore = Score FROM CustomerAnalyticsCompany WHERE CustomerID=@CustomerId AND IsActive=1
	IF @CompanyScore > 0 
		SET @HasCompanyScore = 1
END	
ELSE
BEGIN 
	SELECT  @CompanyScore = nl.CommercialDelphiScore 
	FROM ExperianNonLimitedResults nl
	WHERE nl.RefNumber=@CompanyRefNum AND nl.IsActive=1
	
	IF @CompanyScore > 0 
		SET @HasCompanyScore = 1
END

IF EXISTS (SELECT * FROM MP_CustomerMarketPlace m INNER JOIN MP_MarketplaceType t ON t.Id = m.MarketPlaceId 
		   WHERE m.CustomerId=@CustomerId AND m.Disabled=0 AND t.InternalId IN ('A7120CB7-4C93-459B-9901-0E95E7281B59',	'A4920125-411F-4BB9-A52D-27E8A00D0A3B', '3FA5E327-FCFD-483B-BA5A-DC1815747A28'))
	SET @HasOnline = 1
				
IF EXISTS (SELECT * FROM MP_CustomerMarketPlace m INNER JOIN MP_MarketplaceType t ON t.Id = m.MarketPlaceId 
		   WHERE m.CustomerId=@CustomerId AND m.Disabled=0 AND t.InternalId = '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF')
	SET @HasBank = 1

SELECT @NumOfHmrc = count(*) FROM MP_CustomerMarketPlace m INNER JOIN MP_MarketplaceType t ON t.Id = m.MarketPlaceId 
WHERE m.CustomerId=@CustomerId AND m.Disabled=0 AND t.InternalId = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'

IF @NumOfHmrc > 0
	SET @HasHmrc = 1

SELECT @LastBankHmrcUpdateDate = min(m.UpdatingEnd)
FROM MP_CustomerMarketPlace m INNER JOIN MP_MarketplaceType t ON t.Id = m.MarketPlaceId 
WHERE m.CustomerId=@CustomerId AND m.Disabled=0 AND t.InternalId IN ('AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA', '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF')

DECLARE @MinApprovalAmount INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name='MedalMinOffer')
DECLARE @MedalDaysOfMpRelevancy INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name='MedalDaysOfMpRelevancy')

SELECT
 @IsLimited AS IsLimited, 
 @HasOnline AS HasOnline, 
 @HasHmrc AS HasHmrc, 
 @HasBank AS HasBank, 
 @HasCompanyScore AS HasCompanyScore, 
 @HasPersonalScore AS HasPersonalScore, 
 @NumOfHmrc AS NumOfHmrc, 
 @LastBankHmrcUpdateDate AS LastBankHmrcUpdateDate,
 @MedalDaysOfMpRelevancy AS MedalDaysOfMpRelevancy,
 @MinApprovalAmount AS MinApprovalAmount
END 
GO
