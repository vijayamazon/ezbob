SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_GetMedalInputParams') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetMedalInputParams AS SELECT 1')
GO

ALTER PROCEDURE AV_GetMedalInputParams
@CustomerId INT
AS
BEGIN

	DECLARE @RegistrationDate DATETIME
	DECLARE @MaritalStatus NVARCHAR(100)
	DECLARE @TypeOfBusiness NVARCHAR(100)
	
	SELECT @RegistrationDate=GreetingMailSentDate, @MaritalStatus = MaritalStatus, @TypeOfBusiness = TypeOfBusiness 
	FROM Customer 
	WHERE Id=@CustomerId
	
	-- BusinessScore IncorporationDate TangibleEquity CurrentBalanceSum taken from CustomerAnalyticsCompany
	DECLARE @BusinessScore INT = 0
	DECLARE @IncorporationDate DATETIME = NULL
	DECLARE @TangibleEquity DECIMAL(18,4) = 0
	DECLARE @CurrentBalanceSum DECIMAL(18,4) = 0
	
	IF @TypeOfBusiness IN ('Limited', 'LLP')
	BEGIN
		SELECT @BusinessScore = Score, @IncorporationDate = IncorporationDate, @TangibleEquity = TangibleEquity, @CurrentBalanceSum = CurrentBalanceSum  
		FROM CustomerAnalyticsCompany 
		WHERE CustomerID = @CustomerId AND IsActive=1
	END
	ELSE
	BEGIN
		SELECT @IncorporationDate = nl.IncorporationDate, @BusinessScore = nl.CommercialDelphiScore FROM Customer c INNER JOIN Company co ON c.CompanyId=co.Id INNER JOIN ExperianNonLimitedResults nl ON co.ExperianRefNum = nl.RefNumber
		WHERE c.Id = @CustomerId AND nl.IsActive=1
	END
			
	-- Minimal Consumer/Directors score
	DECLARE @ConsumerScore INT 
	SELECT @ConsumerScore = ISNULL(MIN(x.ExperianConsumerScore), 0) FROM
	(
	SELECT ExperianConsumerScore FROM Customer WHERE Id=@CustomerId AND ExperianConsumerScore IS NOT NULL
	UNION
	SELECT ExperianConsumerScore FROM Director WHERE CustomerId=@CustomerId AND ExperianConsumerScore IS NOT NULL
	) x
	
	
	--NumOfOnTimeLoans
	DECLARE @NumOfOnTimeLoans INT 
	;WITH
	late_loans AS (
	    SELECT DISTINCT
	        lst.LoanID
	    FROM
	        LoanScheduleTransaction lst
	        INNER JOIN Loan l ON lst.LoanId = l.Id AND l.CustomerId = @CustomerID
	    WHERE
	        lst.StatusBefore IN ('Late', 'Paid')
	        OR
	        lst.StatusAfter IN ('Late', 'Paid')
	)
	SELECT
	    @NumOfOnTimeLoans = ISNULL(COUNT(DISTINCT l.Id), 0)
	FROM
	    Loan l
	    LEFT JOIN late_loans ll ON l.Id = ll.LoanID
	WHERE
	    ll.LoanID IS NULL
	    AND
	    l.CustomerId = @CustomerID
	  
	
	--NumOfLatePayments
	DECLARE @LateFeeConfigId INT = (SELECT Id FROM ConfigurationVariables WHERE Name = 'LatePaymentCharge')
	DECLARE @NumOfLatePayments INT 
	
	SELECT @NumOfLatePayments = count(lc.Id) 
	FROM Loan l INNER JOIN LoanCharges lc ON l.Id=lc.LoanId 
	WHERE l.CustomerId=@CustomerId AND lc.ConfigurationVariableId=@LateFeeConfigId
	
	--NumOfEarlyPayments
	DECLARE @NumOfEarlyPayments INT 
	
	SELECT @NumOfEarlyPayments = count(ls.Id)
	FROM Loan l INNER JOIN LoanSchedule ls ON l.Id = ls.LoanId
	WHERE l.CustomerId = @CustomerId AND ls.Status = 'PaidEarly'
	
	--HMRC
	DECLARE @NumOfHmrc INT 
	DECLARE @HasHmrc BIT = 0
	DECLARE @HasMoreThenOneHmrc BIT = 0
	
	SELECT @NumOfHmrc = count(mp.Id)
	FROM MP_CustomerMarketPlace mp INNER JOIN MP_MarketplaceType t ON t.Id = mp.MarketPlaceId 
	WHERE t.InternalId='AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA' AND mp.CustomerId=@CustomerId AND mp.Disabled = 0
	
	DECLARE @FCFFactor DECIMAL(18,4)
	DECLARE @HmrcRevenues DECIMAL(18,4) = 0
	DECLARE @HmrcEbida DECIMAL(18,4) = 0
	DECLARE @HmrcValueAdded DECIMAL(18,4) = 0
	
	IF @NumOfHmrc > 0 SET @HasHmrc = 1
	IF @NumOfHmrc > 1 SET @HasMoreThenOneHmrc = 1
	IF @NumOfHmrc = 1
		SELECT @HmrcEbida = v.Ebida, @HmrcRevenues = v.Revenues, @HmrcValueAdded = v.TotalValueAdded
		FROM MP_VatReturnSummary v INNER JOIN MP_CustomerMarketPlace mp ON v.CustomerMarketplaceID = mp.Id
		WHERE v.IsActive=1 AND mp.CustomerId=@CustomerId AND mp.Disabled=0
	
	
	SELECT @FCFFactor = CAST(Value AS DECIMAL(18, 4)) FROM ConfigurationVariables WHERE Name='FCFFactor'
	
	--NetWorth
	
	--Zoopla
	DECLARE @ZooplaValue INT 
	SELECT @ZooplaValue = isnull(sum(CASE WHEN z.ZooplaEstimateValue = 0 THEN z.AverageSoldPrice1Year ELSE z.ZooplaEstimateValue END), 0)  
	FROM CustomerAddress ca INNER JOIN Zoopla z ON z.CustomerAddressId = ca.addressId 
	WHERE CustomerId=@CustomerId AND IsOwnerAccordingToLandRegistry=1
	
	--Mortgages
	DECLARE @ServiceLogId BIGINT
	
	SELECT TOP 1
		@ServiceLogId = Id
	FROM
		MP_ServiceLog l
	WHERE
		l.CustomerId = @CustomerId
		AND
		l.ServiceType = 'Consumer Request'
		AND
		l.DirectorId IS NULL
	ORDER BY
		l.InsertDate DESC,
		l.Id DESC
	
	IF @ServiceLogId IS NULL
	BEGIN
		SELECT TOP 1
		   @ServiceLogId=l.Id
		FROM
		 Customer c 
		 INNER JOIN CustomerAddress a ON a.CustomerId = c.Id AND a.addressType=1
		 INNER JOIN MP_ServiceLog l on
		  l.Firstname = c.FirstName AND
		  l.Surname = c.Surname AND 
		  l.DateOfBirth = c.DateOfBirth AND
		  l.Postcode = a.Postcode AND
		  l.ServiceType = 'Consumer Request'
		  
		  WHERE
		   c.Id=@CustomerId
		  ORDER BY
		   l.InsertDate DESC,
		   l.Id DESC
	END
	
	DECLARE @Mortages INT
	SELECT @Mortages = isnull(sum(Balance),0)
	FROM ExperianConsumerDataCais c INNER JOIN ExperianConsumerData d ON d.Id = c.ExperianConsumerDataId 
	WHERE d.ServiceLogId=@ServiceLogId 
	AND AccountType IN ('03','16','25','30','31','32','33','34','35','69') 
	AND MatchTo=1 
	AND AccountStatus <> 'S'
	
	--First Repayment Date
	DECLARE @FirstRepaymentDate DATETIME = (SELECT TOP 1 ls.[Date] 
											FROM Loan l INNER JOIN LoanSchedule ls ON ls.LoanId = l.Id 
											WHERE l.CustomerId=@CustomerId ORDER BY ls.[Date])
	
	--Num of ebay amazon and paypal stores
	DECLARE @NumOfStores INT 
	SELECT @NumOfStores = isnull(count(mp.Id), 0) FROM MP_CustomerMarketPlace mp INNER JOIN MP_MarketplaceType t ON t.Id = mp.MarketPlaceId
	WHERE mp.CustomerId=@CustomerId AND mp.Disabled=0
	AND t.InternalId IN ('A7120CB7-4C93-459B-9901-0E95E7281B59', 'A4920125-411F-4BB9-A52D-27E8A00D0A3B', '3FA5E327-FCFD-483B-BA5A-DC1815747A28')
	
	
	-- Return All Params
	SELECT 
		@BusinessScore AS BusinessScore,
		@IncorporationDate AS IncorporationDate,
		@TangibleEquity AS TangibleEquity,
		@ConsumerScore AS ConsumerScore,
		@RegistrationDate AS RegistrationDate,
		@MaritalStatus AS MaritalStatus,
		@TypeOfBusiness AS TypeOfBusiness,
		@NumOfOnTimeLoans AS NumOfOnTimeLoans,
		@NumOfLatePayments AS NumOfLatePayments,
		@NumOfEarlyPayments AS NumOfEarlyPayments,
		@HasHmrc AS HasHmrc,
		@HasMoreThenOneHmrc AS HasMoreThenOneHmrc,
		@HmrcRevenues AS HmrcRevenues,
		@HmrcEbida AS HmrcEbida,
		@HmrcValueAdded AS HmrcValueAdded,
		@FCFFactor AS FCFFactor,
		@CurrentBalanceSum AS CurrentBalanceSum,
		@ZooplaValue AS ZooplaValue,
		@Mortages AS Mortages,
		@FirstRepaymentDate AS FirstRepaymentDate,
		@NumOfStores AS NumOfStores
END
GO