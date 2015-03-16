
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MainStrategyGetPersonalInfo]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MainStrategyGetPersonalInfo]
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
		LEFT OUTER JOIN Company ON CustomerId = Customer.Id
	WHERE
		Customer.Id = @CustomerId
		
END
GO

