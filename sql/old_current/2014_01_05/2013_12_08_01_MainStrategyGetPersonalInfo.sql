IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MainStrategyGetPersonalInfo]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MainStrategyGetPersonalInfo]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MainStrategyGetPersonalInfo] 
	(@CustomerId INT)
AS
BEGIN
	DECLARE
		@CustomerStatusIsEnabled BIT,
		@CustomerStatusIsWarning BIT,
		@NumOfMps INT,
		@NumOfLoans INT

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
		IsOffline,
		Name AS CustomerEmail,
		TypeOfBusiness AS CompanyType,
		LimitedRefNum,
		NonLimitedRefNum,
		FirstName, 
		Surname,
		Gender,
		DateOfBirth,
		ResidentialStatus AS HomeOwner,
		@NumOfMps AS NumOfMps,
		TimeAtAddress,
		AccountNumber,
		SortCode,
		GreetingMailSentDate AS RegistrationDate,
		BankAccountType,
		@NumOfLoans AS NumOfLoans
	FROM
		Customer
	WHERE
		Id = @CustomerId
		
END
GO
