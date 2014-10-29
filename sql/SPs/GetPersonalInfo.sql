IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPersonalInfo]') AND TYPE IN (N'P', N'PC'))
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
		@CustomerStatusName NVARCHAR(100),
		@NumOfHmrcMps INT
		
	SELECT 
		@CustomerStatusIsEnabled = IsEnabled, 
		@CustomerStatusIsWarning = IsWarning,
		@CustomerStatusName = CustomerStatuses.Name
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
		@NumOfHmrcMps = COUNT(1) 
	FROM 
		MP_CustomerMarketPlace, 
		MP_MarketplaceType 
	WHERE 
		CustomerId = @CustomerId AND 
		MP_CustomerMarketPlace.MarketPlaceId = MP_MarketplaceType.Id AND 
		MP_MarketplaceType.Name = 'HMRC'
	
	SELECT
		@CustomerStatusIsEnabled AS CustomerStatusIsEnabled,
		@CustomerStatusIsWarning AS CustomerStatusIsWarning,
		@CustomerStatusName AS CustomerStatusName,		
		IsOffline,
		Name AS CustomerEmail,
		Company.TypeOfBusiness AS CompanyType,
		FirstName, 
		Surname,
		Gender,
		DateOfBirth,
		CustomerPropertyStatuses.IsOwnerOfMainAddress AS IsOwnerOfMainAddress,
		CustomerPropertyStatuses.IsOwnerOfOtherProperties AS IsOwnerOfOtherProperties,
		CustomerPropertyStatuses.Description AS PropertyStatusDescription,
		@NumOfMps AS NumOfMps,
		Customer.TimeAtAddress,
		AccountNumber,
		SortCode,
		GreetingMailSentDate AS RegistrationDate,
		BankAccountType,
		@NumOfLoans AS NumOfLoans,
		Customer.TypeOfBusiness,
		@NumOfHmrcMps AS NumOfHmrcMps,
		Customer.IsAlibaba,
		Customer.BrokerID AS BrokerId,
		Customer.LastStartedMainStrategyEndTime
	FROM
		CustomerPropertyStatuses,
		Customer
		LEFT OUTER JOIN Company ON Company.Id = Customer.CompanyId
	WHERE
		Customer.Id = @CustomerId AND
		Customer.PropertyStatusId = CustomerPropertyStatuses.Id
END
GO
