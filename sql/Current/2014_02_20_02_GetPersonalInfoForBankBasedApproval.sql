IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPersonalInfoForBankBasedApproval]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPersonalInfoForBankBasedApproval]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPersonalInfoForBankBasedApproval] 
	(@CustomerId INT,
	 @NumOfMonthsToLookForDefaults INT)
AS
BEGIN
	DECLARE 
		@AmlId BIGINT,
		@FirstName NVARCHAR(250),
		@LastName NVARCHAR(250),
		@DefaultCount INT,
		@CompanyId INT,
		@ReferenceSource NVARCHAR(200),
		@HasNonYodleeMarketplace BIT,
		@IsOffline BIT,
		@DateOfBirth DateTime,
		@ResidentialStatus NVARCHAR(250),
		@ExperianScore INT,
		@EarliestTransactionDate DATETIME
		
	SELECT TOP 1
		@AmlId = Id
	FROM
		MP_ServiceLog
	WHERE
		CustomerId = @CustomerId AND
		ServiceType = 'AML A check'
	ORDER BY
		InsertDate DESC
				
	SELECT
		@FirstName = FirstName,
		@LastName = Surname,
		@CompanyId = CompanyId,
		@ReferenceSource = ReferenceSource,
		@IsOffline = IsOffline,
		@DateOfBirth = DateOfBirth,
		@ResidentialStatus = ResidentialStatus
	FROM
		Customer
	WHERE
		Id = @CustomerId		
		
	SELECT
		@DefaultCount = COUNT(1)
	FROM
		ExperianDefaultAccount
	WHERE
		CustomerId = @CustomerId AND
		DATEDIFF(MONTH, Date, GETUTCDATE()) < @NumOfMonthsToLookForDefaults
	
	IF EXISTS (SELECT 1 FROM MP_CustomerMarketplace, MP_MarketplaceType WHERE CustomerId = @CustomerId AND MarketplaceId = MP_MarketplaceType.Id AND MP_MarketplaceType.Name <> 'Yodlee')
	BEGIN
		SELECT @HasNonYodleeMarketplace = 1
	END
	ELSE
	BEGIN
		SELECT @HasNonYodleeMarketplace = 0
	END
	
	SELECT
		@ExperianScore = ExperianScore
	FROM
		MP_ExperianDataCache
	WHERE
		CustomerId = @CustomerId AND
		DirectorId = 0
		
	SELECT
		@EarliestTransactionDate = MIN(PostDate)
	FROM 
		MP_YodleeOrderItemBankTransaction 
	WHERE 
		OrderItemId IN 
		(
			SELECT 
				Id 
			FROM 
				MP_YodleeOrderItem 
			WHERE 
				OrderId IN 
				(
					SELECT 
						Id 
					FROM 
						MP_YodleeOrder 
					WHERE 
						CustomerMarketPlaceId IN
						(
							SELECT 
								Id
							FROM
								MP_CustomerMarketplace
							WHERE
								CustomerId = @CustomerId
						)
				)
		)

	SELECT
		(SELECT ResponseData FROM MP_ServiceLog WHERE Id = @AmlId) AS AmlData,
		@FirstName AS FirstName,
		@LastName AS Surname,
		(SELECT JsonPacket FROM MP_ExperianDataCache WHERE Id = @CompanyId) AS CompanyData,
		CAST((CASE @DefaultCount WHEN 0 THEN 0 ELSE 1 END) AS BIT) AS HasDefaultAccounts,
		CAST((CASE @ReferenceSource WHEN 'liqcen' THEN 1 ELSE 0 END) AS BIT) AS IsCustomerViaBroker,
		@HasNonYodleeMarketplace AS HasNonYodleeMarketplace,
		@IsOffline AS IsOffline,
		@DateOfBirth AS DateOfBirth,
		CAST((CASE @ResidentialStatus WHEN 'Home owner' THEN 1 ELSE 0 END) AS BIT) AS IsHomeOwner,
		@ExperianScore AS ExperianScore,
		ISNULL(@EarliestTransactionDate, GETUTCDATE()) AS EarliestTransactionDate
END
GO
