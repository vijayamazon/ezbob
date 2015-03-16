IF OBJECT_ID('AV_GetCustomerMarketPlaces') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetCustomerMarketPlaces AS SELECT 1')
GO

ALTER PROCEDURE AV_GetCustomerMarketPlaces
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON

	SELECT mp.Id mpId, mp.DisplayName Name, t.Name Type, mp.OriginationDate FROM MP_CustomerMarketPlace mp 
	INNER JOIN MP_MarketplaceType t 
	ON t.Id = mp.MarketPlaceId 
	WHERE CustomerId=@CustomerId 
	AND mp.Disabled = 0 
	AND (t.IsPaymentAccount = 0 OR t.InternalId='3FA5E327-FCFD-483B-BA5A-DC1815747A28')
	
	SET NOCOUNT OFF
END
GO

IF OBJECT_ID('AV_GetAnalysisFunctions') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetAnalysisFunctions AS SELECT 1')
GO

ALTER PROCEDURE AV_GetAnalysisFunctions
 @CustomerMarketPlaceId INT 
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @LastUpdated DATETIME 
    SET @LastUpdated = (SELECT max(Updated) FROM MP_AnalyisisFunctionValues WHERE CustomerMarketPlaceId = @CustomerMarketPlaceId)
    SELECT v.Updated, v.Value, f.Name FunctionName, t.Id TimePeriod	
	FROM MP_AnalyisisFunctionValues v
	INNER JOIN MP_AnalyisisFunction f ON v.AnalyisisFunctionId = f.Id
	INNER JOIN MP_AnalysisFunctionTimePeriod t ON v.AnalysisFunctionTimePeriodId=t.Id 
	INNER JOIN MP_ValueType vt ON vt.Id = f.ValueTypeId
	AND v.CustomerMarketPlaceId = @CustomerMarketPlaceId 
	AND v.Updated = @LastUpdated
	AND vt.InternalId = '97594E98-6B09-46AB-83ED-618678B327BE'
	SET NOCOUNT OFF
END
GO

IF OBJECT_ID('AV_GetExperianScore') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetExperianScore AS SELECT 1')
GO

ALTER PROCEDURE AV_GetExperianScore
 @CustomerId INT 
AS
BEGIN
	SET NOCOUNT ON
	SELECT TOP 1 JsonPacket, ExperianScore 
	FROM MP_ExperianDataCache 
	WHERE CustomerId=@CustomerId AND ExperianScore IS NOT NULL
	ORDER BY LastUpdateDate DESC
SET NOCOUNT OFF
END
GO

IF OBJECT_ID('AV_WasLoanApproved') IS NULL
	EXECUTE('CREATE PROCEDURE AV_WasLoanApproved AS SELECT 1')
GO

ALTER PROCEDURE AV_WasLoanApproved
 @CustomerId INT 
AS
BEGIN
	SET NOCOUNT ON
	IF EXISTS (SELECT * 
			   FROM CashRequests 
			   WHERE IdCustomer = @CustomerId 
			   AND (SystemDecision = 'Approved' OR UnderwriterDecision='Approved')
			  )
		SELECT 'true'
	ELSE
		SELECT 'false'		
	SET NOCOUNT OFF
END
GO

IF OBJECT_ID('AV_HasDefaultAccounts') IS NULL
	EXECUTE('CREATE PROCEDURE AV_HasDefaultAccounts AS SELECT 1')
GO

ALTER PROCEDURE AV_HasDefaultAccounts
 @CustomerId INT,
 @MinDefBalance INT,
 @Months INT
AS
BEGIN
	IF EXISTS (SELECT * 
			   FROM ExperianDefaultAccount d 
			   WHERE CustomerId = @CustomerId 
			   AND dateadd(month, @Months, d.Date) > getdate() 
			   AND CurrentDefBalance > @MinDefBalance
			  )
		SELECT 'true'
	ELSE
	    SELECT 'false'
END
GO

