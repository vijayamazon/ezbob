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
		@CustomerStatusName NVARCHAR(100)
		
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
		@CustomerStatusIsEnabled AS CustomerStatusIsEnabled,
		@CustomerStatusIsWarning AS CustomerStatusIsWarning,
		@CustomerStatusName AS CustomerStatusName,		
		IsOffline,
		CAST(CASE WHEN BrokerID IS NULL THEN 0 ELSE 1 END AS BIT) AS IsBrokerCustomer,
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
		@NumOfLoans AS NumOfLoans,
		Customer.TypeOfBusiness
	FROM
		Customer
		LEFT OUTER JOIN Company ON Company.Id = Customer.CompanyId
	WHERE
		Customer.Id = @CustomerId
END
GO
