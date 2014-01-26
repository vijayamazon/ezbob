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
    SELECT v.Updated, v.Value, f.Name FunctionName, t.Id TimePeriod, mpt.Name MarketPlaceName	
	FROM MP_AnalyisisFunctionValues v
	INNER JOIN MP_AnalyisisFunction f ON v.AnalyisisFunctionId = f.Id
	INNER JOIN MP_AnalysisFunctionTimePeriod t ON v.AnalysisFunctionTimePeriodId=t.Id 
	INNER JOIN MP_ValueType vt ON vt.Id = f.ValueTypeId
	INNER JOIN MP_MarketplaceType mpt ON mpt.Id = f.MarketPlaceId
	AND v.CustomerMarketPlaceId = @CustomerMarketPlaceId 
	AND v.Updated = @LastUpdated
	AND vt.InternalId = '97594E98-6B09-46AB-83ED-618678B327BE'
	SET NOCOUNT OFF
END

GO
