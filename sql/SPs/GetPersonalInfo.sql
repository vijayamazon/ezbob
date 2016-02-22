IF OBJECT_ID('GetPersonalInfo') IS NULL 
	EXECUTE('CREATE PROCEDURE GetPersonalInfo AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetPersonalInfo
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @Yodlee UNIQUEIDENTIFIER = '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF'
	DECLARE @HMRC   UNIQUEIDENTIFIER = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'

	------------------------------------------------------------------------------

	DECLARE
		@CustomerStatusIsEnabled BIT,
		@CustomerStatusIsWarning BIT,
		@NumOfMps INT,
		@NumOfLoans INT,
		@CustomerStatusName NVARCHAR(100),
		@NumOfHmrcMps INT,
		@NumOfYodleeMps INT,
		@EarliestYodleeLastUpdateDate DATETIME,
		@EarliestHmrcLastUpdateDate DATETIME,
		@NumOfPreviousApprovals INT
		
	------------------------------------------------------------------------------

	SELECT 
		@CustomerStatusIsEnabled = IsEnabled,
		@CustomerStatusIsWarning = IsWarning,
		@CustomerStatusName = CustomerStatuses.Name
	FROM
		CustomerStatuses,
		Customer c
	WHERE
		c.CollectionStatus = CustomerStatuses.Id AND
		c.Id = @CustomerId
	
	------------------------------------------------------------------------------

	SELECT 
		@NumOfMps = COUNT(cmp.Id)
	FROM 
		MP_CustomerMarketPlace cmp
	WHERE 
		CustomerId = @CustomerId

	------------------------------------------------------------------------------

	SELECT
		@NumOfLoans = COUNT(1)
	FROM
		Loan
	WHERE
		CustomerId = @CustomerId

	------------------------------------------------------------------------------

	SELECT
		@NumOfYodleeMps = COUNT(1),
		@EarliestYodleeLastUpdateDate = MIN(m.UpdatingEnd)
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t
			ON m.MarketPlaceId = t.Id
			AND t.InternalId = @Yodlee
	WHERE
		m.CustomerId = @CustomerId

	------------------------------------------------------------------------------

	SELECT		
		@NumOfHmrcMps = COUNT(1),
		@EarliestHmrcLastUpdateDate = MIN(UpdatingEnd)
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t
			ON m.MarketPlaceId = t.Id
			AND t.InternalId = @HMRC
		INNER JOIN MP_VatReturnRecords r ON m.Id = r.CustomerMarketPlaceId
		INNER JOIN Business b
			ON r.BusinessId = b.Id
			AND b.BelongsToCustomer = 1
	WHERE
		m.CustomerId = @CustomerId

	------------------------------------------------------------------------------

	SELECT
		@NumOfPreviousApprovals = COUNT(*)
	FROM
		DecisionHistory h
	WHERE
		h.CustomerId = @CustomerId
		AND
		h.Action = 'Approve'

	------------------------------------------------------------------------------

	SELECT
		@CustomerStatusIsEnabled AS CustomerStatusIsEnabled,
		@CustomerStatusIsWarning AS CustomerStatusIsWarning,
		@CustomerStatusName AS CustomerStatusName,
		IsOffline,
		c.Name AS CustomerEmail,
		Company.TypeOfBusiness AS CompanyType,
		FirstName, 
		Surname,
		Gender,
		DateOfBirth,
		CAST(isnull(ps.IsOwnerOfMainAddress, 0) AS BIT) AS IsOwnerOfMainAddress,
		CAST(isnull(ps.IsOwnerOfOtherProperties, 0) AS BIT) AS IsOwnerOfOtherProperties,
		ps.Description AS PropertyStatusDescription,
		@NumOfMps AS NumOfMps,
		c.TimeAtAddress,
		AccountNumber,
		SortCode,
		GreetingMailSentDate AS RegistrationDate,
		BankAccountType,
		@NumOfLoans AS NumOfLoans,
		c.TypeOfBusiness,
		@NumOfHmrcMps AS NumOfHmrcMps,
		c.IsAlibaba,
		c.BrokerID AS BrokerId,
		c.LastStartedMainStrategyEndTime,
		@NumOfYodleeMps AS NumOfYodleeMps,
		@EarliestYodleeLastUpdateDate AS EarliestYodleeLastUpdateDate,
		@EarliestHmrcLastUpdateDate AS EarliestHmrcLastUpdateDate,
		c.IsTest,
		c.FilledByBroker,
		ISNULL(@NumOfPreviousApprovals, 0) AS NumOfPreviousApprovals,
		c.Fullname AS FullName,
		o.Name AS Origin,
		c.OriginID
	FROM
		Customer c
		LEFT JOIN CustomerPropertyStatuses ps ON c.PropertyStatusId = ps.Id
		LEFT JOIN Company ON Company.Id = c.CompanyId
		LEFT JOIN CustomerOrigin o ON o.CustomerOriginID = c.OriginID 
	WHERE
		c.Id = @CustomerId

	------------------------------------------------------------------------------
END
GO
