IF OBJECT_ID('AV_GetRejectionData') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetRejectionData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE AV_GetRejectionData
	@CustomerId INT 
AS
BEGIN

--Customer Status
DECLARE @CustomerStatus NVARCHAR(30) 
DECLARE @IsLimited BIT = 0
DECLARE @IsBrokerClient BIT = 0

SELECT @CustomerStatus=cs.Name, @IsLimited = CASE 
	WHEN c.TypeOfBusiness='Limited' THEN 1
	WHEN c.TypeOfBusiness='LLP' THEN 1
	ELSE 0 END,
	@IsBrokerClient = CASE WHEN c.BrokerID IS NULL THEN 0 ELSE 1 END
	
FROM Customer c INNER JOIN CustomerStatuses cs ON cs.Id = c.CollectionStatus 
WHERE c.Id = @CustomerId

--Max Experian Consumer, Directors Score
DECLARE @ConsumerServiceLogId BIGINT 
EXEC GetExperianConsumerServiceLog @CustomerId, @ConsumerServiceLogId OUTPUT
DECLARE @ExperianScore INT

SELECT @ExperianScore = isnull(max(x.ExperianConsumerScore),0) FROM
(
	SELECT ExperianConsumerScore FROM Director WHERE CustomerId=@CustomerId
	UNION
	SELECT BureauScore AS ExperianConsumerScore FROM ExperianConsumerData WHERE ServiceLogId = @ConsumerServiceLogId
) x

--Was Approved For Loan
DECLARE @WasApproved BIT = 0
IF EXISTS (SELECT * FROM CashRequests WHERE IdCustomer = @CustomerId AND UnderwriterDecision='Approved')
BEGIN		   
	SET @WasApproved = 1
END	

--Company Score
DECLARE @ExperianLtd INT = ISNULL((
	SELECT MAX(ISNULL(e.ExperianLtdID, 0))
	FROM Customer c
	LEFT JOIN Company co ON c.CompanyId = co.Id
	LEFT JOIN ExperianLtd e ON co.ExperianRefNum = e.RegisteredNumber
	WHERE c.Id = @CustomerId
), 0)

DECLARE @CompanyScore INT = 0
DECLARE @CompanyMaxScore INT = 0

--incorporation date
DECLARE @IncorporationDate DATETIME

IF @IsLimited = 1
BEGIN
	
	SELECT @CompanyMaxScore = isnull(MaxScore,0), @CompanyScore = isnull(Score,0), @IncorporationDate = IncorporationDate 
	FROM CustomerAnalyticsCompany 
	WHERE CustomerID=@CustomerId AND IsActive=1

	IF @CompanyScore < @CompanyMaxScore
		SET @CompanyScore = @CompanyMaxScore
END  
ELSE
BEGIN
	SELECT @CompanyScore = isnull(e.RiskScore, 0), @IncorporationDate = e.IncorporationDate 
	FROM Customer c 
	LEFT JOIN Company co ON c.CompanyId = co.Id 
	LEFT JOIN ExperianNonLimitedResults e ON e.RefNumber = co.ExperianRefNum 
	WHERE e.IsActive=1 AND c.Id = @CustomerId
END	

-- Has Company Files
DECLARE @HasCompanyFiles BIT = 0
IF EXISTS ( SELECT * FROM MP_CustomerMarketPlace mp INNER JOIN MP_MarketplaceType m ON m.Id = mp.MarketPlaceId 
			WHERE mp.CustomerId	= @CustomerId AND m.InternalId='1C077670-6D6C-4CE9-BEBC-C1F9A9723908' AND mp.Disabled=0)
BEGIN			
	SET @HasCompanyFiles = 1
END

--Has Mp errors
DECLARE @HasErrorMp BIT = 0
IF EXISTS (SELECT * FROM MP_CustomerMarketPlace mp
		   WHERE mp.CustomerId = @CustomerId 
		   AND mp.UpdateError IS NOT NULL 
		   AND mp.UpdateError <> '' 
		   AND mp.Disabled=0)
BEGIN			
	SET @HasErrorMp = 1
END


SELECT @CustomerStatus AS CustomerStatus, 
	   @ExperianScore AS ExperianScore, 
	   @CompanyScore AS CompanyScore, 
	   @WasApproved AS WasApproved, 
	   @IsBrokerClient AS IsBrokerClient,
	   @HasErrorMp AS HasErrorMp,
	   @HasCompanyFiles AS HasCompanyFiles,
	   @IncorporationDate AS IncorporationDate
END

GO