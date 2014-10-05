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
DECLARE @CustomerStatus NVARCHAR(30) = (SELECT cs.Name FROM Customer c INNER JOIN CustomerStatuses cs ON cs.Id = c.CollectionStatus WHERE c.Id = @CustomerId)

--Experian Score
DECLARE @ConsumerServiceLogId BIGINT 
EXEC GetExperianConsumerServiceLog @CustomerId, @ConsumerServiceLogId OUTPUT
DECLARE @ExperianScore INT = (SELECT isnull(BureauScore, 0) FROM ExperianConsumerData WHERE ServiceLogId = @ConsumerServiceLogId)

--Was Approved For Loan
DECLARE @WasApproved BIT = 0
IF EXISTS (SELECT * 
		   FROM CashRequests 
		   WHERE IdCustomer = @CustomerId 
		   AND (SystemDecision = 'Approved' OR UnderwriterDecision='Approved'))
BEGIN		   
	SET @WasApproved = 1
END	

-- Defaults	
DECLARE @DefaultAccountAmount INT 
DECLARE @DefaultAccountsNum INT 		
SELECT @DefaultAccountAmount = isnull(sum(c.CurrentDefBalance), 0) , @DefaultAccountsNum = isnull(count(c.Id), 0)
FROM ExperianConsumerData e INNER JOIN ExperianConsumerDataCais c ON c.ExperianConsumerDataId = e.Id
WHERE e.ServiceLogId = @ConsumerServiceLogId AND c.MatchTo = 1 AND c.AccountStatus <> 'S' AND c.WorstStatus IN ('8', '9')

--Company Defaults and Company Score
DECLARE @ExperianLtd INT = (SELECT isnull(max(e.ExperianLtdID), 0) FROM Customer c 
LEFT JOIN Company co ON c.CompanyId = co.Id 
LEFT JOIN ExperianLtd e ON co.ExperianRefNum = e.RegisteredNumber)
DECLARE @DefaultCompanyAccountAmount INT = 0
DECLARE @DefaultCompanyAccountsNum INT = 0
DECLARE @CompanyScore INT = 0

IF @ExperianLtd > 0
BEGIN
	SELECT @DefaultCompanyAccountAmount = isnull(sum(c.DefaultBalance), 0), @DefaultCompanyAccountsNum = isnull(count(c.ExperianLtdDL97ID), 0) FROM ExperianLtdDL97 c
	WHERE c.ExperianLtdID=@ExperianLtd AND c.AccountState='D'
	
	SET @CompanyScore = (SELECT isnull(CommercialDelphiScore, 0) FROM ExperianLtd WHERE ExperianLtdID = @ExperianLtd)
	
END  
ELSE
BEGIN
	SET @CompanyScore = (SELECT isnull(e.RiskScore, 0) FROM Customer c 
	LEFT JOIN Company co ON c.CompanyId = co.Id 
	LEFT JOIN ExperianNonLimitedResults e ON e.RefNumber = co.ExperianRefNum 
	WHERE e.IsActive=1)
END	

-- Has Company Files
DECLARE @HasCompanyFiles BIT = 0
IF EXISTS ( SELECT mp.Id  FROM MP_CustomerMarketPlace mp INNER JOIN MP_MarketplaceType m ON m.Id = mp.MarketPlaceId 
			WHERE mp.CustomerId	= @CustomerId AND m.InternalId='1C077670-6D6C-4CE9-BEBC-C1F9A9723908' )
BEGIN			
	SET @HasCompanyFiles = 1
END

--Has Mp errors
DECLARE @HasErrorMp BIT = 0
IF EXISTS ( SELECT mp.Id  FROM MP_CustomerMarketPlace mp
			WHERE mp.CustomerId	= @CustomerId 
			AND (mp.UpdateError IS NOT NULL OR mp.UpdateError <> ''))
BEGIN			
	SET @HasErrorMp = 1
END


SELECT @CustomerStatus AS CustomerStatus, 
	   @ExperianScore AS ExperianScore, 
	   @CompanyScore AS CompanyScore, 
	   @WasApproved AS WasApproved, 
	   @DefaultAccountsNum AS DefaultAccountsNum, 
	   @DefaultAccountAmount AS DefaultAccountAmount,
	   @DefaultCompanyAccountsNum AS DefaultCompanyAccountsNum,
	   @DefaultCompanyAccountAmount AS DefaultCompanyAccountAmount,
	   @HasErrorMp AS HasErrorMp,
	   @HasCompanyFiles AS HasCompanyFiles
END
GO