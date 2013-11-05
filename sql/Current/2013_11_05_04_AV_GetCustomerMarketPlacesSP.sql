IF OBJECT_ID('AV_GetCustomerMarketPlaces') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetCustomerMarketPlaces AS SELECT 1')
GO

ALTER PROCEDURE AV_GetCustomerMarketPlaces
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON

	SELECT mp.Id mpId, mp.DisplayName Name, t.Name Type FROM MP_CustomerMarketPlace mp 
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

