IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CompanyId' and Object_ID = Object_ID(N'Customer'))
BEGIN 
	ALTER TABLE Customer ADD CompanyId INT NULL
		
	DECLARE @stmt VARCHAR(MAX)
	SET @stmt = 
	'DECLARE @CompanyId INT, @CustomerId INT
	DECLARE cur CURSOR FOR SELECT Id, CustomerId FROM Company		
	OPEN cur
	FETCH NEXT FROM cur INTO @CompanyId, @CustomerId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		UPDATE Customer SET CompanyId = @CompanyId WHERE Id = @CustomerId
		
		FETCH NEXT FROM cur INTO @CompanyId, @CustomerId
	END
	CLOSE cur
	DEALLOCATE cur'
	
	EXEC(@stmt)
END 
GO
	
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CustomerId' and Object_ID = Object_ID(N'Company'))
BEGIN 

	PRINT 'Column should be dropped'
	--ALTER TABLE Company DROP FK_Company_Customer
	--ALTER TABLE Company DROP COLUMN CustomerId
END 
GO



IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPersonalInfo]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPersonalInfo]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPersonalInfo] 
	(@CustomerId INT)
AS
BEGIN
	DECLARE
		@CustomerStatusIsEnabled BIT,
		@CustomerStatusIsWarning BIT,
		@NumOfMps INT,
		@NumOfLoans INT,
		@prevExperianConsumerScore INT
		
	SELECT 
		@prevExperianConsumerScore = MIN(ExperianScore) 
	FROM 
		MP_ExperianDataCache 
	WHERE 
		CustomerId = @CustomerId AND 
		Name IS NOT NULL
	
	SELECT 
		@CustomerStatusIsEnabled = IsEnabled, 
		@CustomerStatusIsWarning = IsWarning
	FROM
		CustomerStatuses,
		Customer
	WHERE
		Customer.CollectionStatus = CustomerStatuses.Id AND
		Customer.Id = @CustomerId
	
	SELECT 
		@NumOfMps = COUNT(cmp.Id)
	FROM 
		MP_CustomerMarketPlace cmp
	WHERE 
		CustomerId = @CustomerId
	
	SELECT
		@NumOfLoans = COUNT(1)
	FROM
		Loan
	WHERE
		CustomerId = @CustomerId
	
	SELECT
		@CustomerStatusIsEnabled AS CustomerStatusIsEnabled,
		@CustomerStatusIsWarning AS CustomerStatusIsWarning,
		@prevExperianConsumerScore AS PrevExperianConsumerScore,
		IsOffline,
		Name AS CustomerEmail,
		Company.TypeOfBusiness AS CompanyType,		
		ExperianRefNum,
		CAST(CASE WHEN LastStartedMainStrategyEndTime IS NULL THEN 0 ELSE 1 END AS BIT) AS MainStrategyExecutedBefore,
		FirstName, 
		Surname,
		Gender,
		DateOfBirth,
		ResidentialStatus AS HomeOwner,
		@NumOfMps AS NumOfMps,
		Customer.TimeAtAddress,
		AccountNumber,
		SortCode,
		GreetingMailSentDate AS RegistrationDate,
		BankAccountType,
		@NumOfLoans AS NumOfLoans
	FROM
		Customer
		LEFT OUTER JOIN Company ON Company.Id = Customer.CompanyId
	WHERE
		Customer.Id = @CustomerId
		
END
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FraudGetDetections]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[FraudGetDetections]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[FraudGetDetections] 
	(@CustomerId INT)
AS
BEGIN

IF OBJECT_ID('tempdb..#tmpCustomerPhones') IS NOT NULL DROP TABLE #tmpCustomerPhones
IF OBJECT_ID('tempdb..#tmpCustomerAddresses') IS NOT NULL DROP TABLE #tmpCustomerAddresses
IF OBJECT_ID('tempdb..#tmpCustomerYodlees') IS NOT NULL DROP TABLE #tmpCustomerYodlees
		
DECLARE @FirstName NVARCHAR(300) = (SELECT FirstName  FROM Customer WHERE Id=@CustomerId)
DECLARE @Surname NVARCHAR(300) = (SELECT Surname  FROM Customer WHERE Id=@CustomerId)
DECLARE @DateOfBirth DATETIME = (SELECT DateOfBirth FROM Customer WHERE Id=@CustomerId)
DECLARE @AccountNumber NVARCHAR(20) = (SELECT AccountNumber FROM Customer WHERE Id=@CustomerId)
DECLARE @SortCode NVARCHAR(20) = (SELECT SortCode FROM Customer WHERE Id=@CustomerId)
DECLARE @Ip NVARCHAR(30) = (SELECT TOP 1 Ip FROM CustomerSession WHERE CustomerId = @CustomerId)
DECLARE @CompanyId INT = (SELECT CompanyId FROM Customer WHERE Id = @CustomerId)
DECLARE @CompanyRefNum NVARCHAR(30) = (SELECT ExperianRefNum FROM Company WHERE Id=@CompanyId)

--customer phones
SELECT DISTINCT x.Phone
INTO #tmpCustomerPhones
FROM
(
SELECT DaytimePhone AS Phone FROM Customer WHERE Id=@CustomerId AND DaytimePhone IS NOT NULL
UNION
SELECT MobilePhone AS Phone FROM Customer WHERE Id=@CustomerId AND MobilePhone IS NOT NULL
UNION
SELECT BusinessPhone AS Phone FROM Company WHERE Id=@CompanyId AND BusinessPhone IS NOT NULL
UNION
SELECT p.Phone AS Phone FROM MP_PayPalPersonalInfo p LEFT JOIN MP_CustomerMarketPlace m ON m.Id = p.CustomerMarketPlaceId 
WHERE m.CustomerId=@CustomerId AND p.Phone IS NOT NULL
UNION 
SELECT e.Phone AS Phone FROM MP_EbayUserAddressData e 
LEFT JOIN MP_EbayUserData d ON d.RegistrationAddressId = e.Id
LEFT JOIN MP_CustomerMarketPlace m ON m.Id = d.CustomerMarketPlaceId
WHERE m.CustomerId=@CustomerId AND e.Phone IS NOT NULL
UNION
SELECT e.Phone2 AS Phone FROM MP_EbayUserAddressData e 
LEFT JOIN MP_EbayUserData d ON d.RegistrationAddressId = e.Id
LEFT JOIN MP_CustomerMarketPlace m ON m.Id = d.CustomerMarketPlaceId
WHERE m.CustomerId=@CustomerId AND e.Phone2 IS NOT NULL
) x

--customer addresses 
SELECT DISTINCT Postcode
INTO #tmpCustomerAddresses
FROM CustomerAddress WHERE CustomerId=@CustomerId

--customer yodlees
SELECT DISTINCT i.accountNumber, i.accountName 
INTO #tmpCustomerYodlees
FROM MP_CustomerMarketPlace m 
INNER JOIN MP_YodleeOrder o ON o.CustomerMarketPlaceId = m.Id 
INNER JOIN MP_YodleeOrderItem i ON i.OrderId = o.Id
WHERE m.CustomerId = @CustomerId

--personal info, bank info, company ref num
SELECT c.Id AS CustomerId FROM Customer c LEFT JOIN Company cc ON cc.Id = c.CompanyId
WHERE c.Id<>@CustomerId 
AND IsTest=0 
AND 
(
c.FirstName LIKE @FirstName 
OR c.Surname LIKE @Surname 
OR c.DateOfBirth = @DateOfBirth 
OR c.DaytimePhone IN (SELECT Phone FROM #tmpCustomerPhones)
OR c.MobilePhone IN (SELECT Phone FROM #tmpCustomerPhones)
OR cc.BusinessPhone IN (SELECT Phone FROM #tmpCustomerPhones)
OR c.AccountNumber = @AccountNumber
OR c.SortCode = @SortCode
OR (cc.ExperianRefNum IS NOT NULL AND cc.ExperianRefNum = @CompanyRefNum)
)

UNION
--paypal phone
SELECT m.CustomerId AS CustomerId FROM MP_PayPalPersonalInfo p LEFT JOIN MP_CustomerMarketPlace m ON m.Id = p.CustomerMarketPlaceId 
WHERE m.CustomerId <> @CustomerId AND Phone IN (SELECT Phone FROM #tmpCustomerPhones)

UNION 
--ebay phone
SELECT m.CustomerId AS CustomerId FROM MP_EbayUserAddressData e 
LEFT JOIN MP_EbayUserData d ON d.RegistrationAddressId = e.Id
LEFT JOIN MP_CustomerMarketPlace m ON m.Id = d.CustomerMarketPlaceId
WHERE m.CustomerId<>@CustomerId AND
(e.Phone IN (SELECT Phone FROM #tmpCustomerPhones)
OR e.Phone2 IN (SELECT Phone FROM #tmpCustomerPhones)
)

UNION
--mp name
SELECT m.CustomerId AS CustomerId  FROM MP_CustomerMarketPlace m 
WHERE CustomerId<>@CustomerId 
AND MarketPlaceId NOT IN (SELECT Id FROM MP_MarketplaceType WHERE Name='Yodlee' OR Name='Sage')
AND DisplayName IN (SELECT DisplayName FROM MP_CustomerMarketPlace WHERE CustomerId=@CustomerId)

UNION
--ip
SELECT CustomerId AS CustomerId FROM CustomerSession WHERE CustomerId <> @CustomerId AND Ip = @Ip

UNION
--director
SELECT CustomerId AS CustomerId FROM Director WHERE CustomerId <> @CustomerId AND (Name LIKE @FirstName OR Surname LIKE @Surname)

UNION
--addresses
SELECT a.CustomerId AS CustomerId  FROM CustomerAddress a WHERE CustomerId<>@CustomerId AND Postcode IN (SELECT Postcode FROM #tmpCustomerAddresses)

UNION 
--director addresses
SELECT c.Id FROM CustomerAddress a 
INNER JOIN Director d ON a.DirectorId=d.id 
INNER JOIN Customer c ON d.CustomerId = c.Id
WHERE c.IsTest=0 AND c.Id<>@CustomerId AND a.Postcode IN (SELECT Postcode FROM #tmpCustomerAddresses)

UNION
--yodlee
SELECT m.CustomerId AS CustomerId 
FROM MP_CustomerMarketPlace m 
INNER JOIN MP_YodleeOrder o ON o.CustomerMarketPlaceId = m.Id 
INNER JOIN MP_YodleeOrderItem i ON i.OrderId = o.Id
WHERE m.CustomerId<>@CustomerId 
AND (i.accountNumber IN (SELECT accountNumber FROM #tmpCustomerYodlees) OR i.accountName IN (SELECT accountName FROM #tmpCustomerYodlees))

DROP TABLE #tmpCustomerPhones
DROP TABLE #tmpCustomerAddresses
DROP TABLE #tmpCustomerYodlees

END
GO




IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCompanyData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCompanyData]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCompanyData] 
	(@CustomerId INT)
AS
BEGIN
	SELECT
		Company.TypeOfBusiness AS CompanyType,		
		Company.ExperianRefNum
	FROM
		Customer,
		Company
	WHERE
		Customer.Id = @CustomerId AND
		Customer.CompanyId = Company.Id
END
GO
