IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMeasurReport]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetMeasurReport]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Oleg Zemskyi
-- Create date: 29.11.2012
-- Description:	CMeasurement report
-- =============================================
CREATE FUNCTION [dbo].[GetMeasurReport]()
RETURNS TABLE 
AS
RETURN 
(
SELECT TOP 100 PERCENT c.Id AS 'Customer ID', c.Name AS ClientName, 
mcmp.DisplayName AS Store, 
       mmt.Name AS Type,      
       Sum(Cast(mcmual.ControlValue AS INT)) AS RowsNumber,       
       dbo.IntToTime(Sum(mcmual.ElapsedRetrieveDataFromExternalService)) AS GetData,
       dbo.IntToTime(Sum(mcmual.ElapsedStoreAggregatedData + mcmual.ElapsedStoreDataToDatabase)) AS StoreData,
       dbo.IntToTime(Sum(mcmual.ElapsedAggregateData)) AS AggregateData,        
       dbo.IntToTime((SUM(mcmual.ElapsedRetrieveDataFromExternalService + 
         mcmual.ElapsedStoreAggregatedData + mcmual.ElapsedStoreDataToDatabase + 
         mcmual.ElapsedAggregateData))) AS Total,
         scuh.StartDate AS 'Customer Strategy Updating Start', 
         scuh.EndDate AS 'Customer Strategy Updating End' ,
         smpuh.StartDate AS 'MarketPlace Strategy Updating Start',
         smpuh.EndDate AS 'MarketPlace Strategy Updating End'      
       
FROM MP_CustomerMarketplaceUpdatingActionLog mcmual
LEFT JOIN MP_CustomerMarketPlaceUpdatingHistory h ON h.Id = mcmual.CustomerMarketplaceUpdatingHistoryRecordId
LEFT JOIN MP_CustomerMarketPlace mcmp ON mcmp.Id = h.CustomerMarketPlaceId
LEFT JOIN Customer c ON c.Id = mcmp.CustomerId
LEFT JOIN MP_MarketplaceType mmt ON mmt.Id = mcmp.MarketPlaceId
LEFT JOIN Strategy_CustomerUpdateHistory scuh ON scuh.CustomerId=c.Id
LEFT JOIN Strategy_MarketPlaceUpdateHistory smpuh ON smpuh.MarketPlaceId=mcmp.Id

WHERE mcmual.ControlValueName = 'TransactionItemsCount'
OR (mcmual.ControlValueName = 'OrdersCount' AND mmt.InternalId = 'A4920125-411F-4BB9-A52D-27E8A00D0A3B')
OR mcmual.ControlValueName = 'TeraPeakOrdersCount'
OR mcmual.ControlValueName = 'eBayOrdersCount'
GROUP BY c.Name, mmt.Name, mcmp.DisplayName, scuh.StartDate, scuh.EndDate, smpuh.StartDate, smpuh.EndDate, c.Id
ORDER by c.Id
)
GO
