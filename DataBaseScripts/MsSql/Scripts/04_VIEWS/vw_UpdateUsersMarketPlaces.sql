IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_UpdateUsersMarketPlaces]'))
DROP VIEW [dbo].[vw_UpdateUsersMarketPlaces]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_UpdateUsersMarketPlaces]
as
SELECT c.Id AS 'CustomerId', c.RefNumber, c.Fullname,  
cmp.DisplayName AS 'MarketplaceName', 
mt.name AS 'TypeOfStore',
cmpuhal.UpdatingStart,cmpuhal.UpdatingEnd, --cmpuh.[Error],--cmpuh.UpdatingTimePassInSeconds,
cmpuhal.ActionName, cmpuhal.ControlValueName,cmpuhal.ControlValue,
[Result]=
CASE
WHEN cmpuhal.[Error] IS NULL
THEN 'Passed'
ELSE
cmpuhal.[Error]
END,	
cmpuhal.UpdatingTimePassInSeconds 
from dbo.MP_CustomerMarketPlace cmp
RIGHT JOIN dbo.MP_CustomerMarketPlaceUpdatingHistory cmpuh
ON cmpuh.CustomerMarketPlaceId = cmp.Id
RIGHT join dbo.MP_MarketplaceType as mt
on cmp.MarketPlaceId=mt.Id
right JOIN Customer c
ON c.Id = cmp.CustomerId
RIGHT JOIN MP_CustomerMarketplaceUpdatingActionLog cmpuhal 
ON cmpuhal.CustomerMarketplaceUpdatingHistoryRecordId = cmpuh.Id
--ORDER BY cmpuhal.UpdatingStart asc
GO
