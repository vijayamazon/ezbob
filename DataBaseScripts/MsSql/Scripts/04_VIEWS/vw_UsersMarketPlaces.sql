IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_UsersMarketPlaces]'))
DROP VIEW [dbo].[vw_UsersMarketPlaces]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create VIEW [dbo].[vw_UsersMarketPlaces]
as
SELECT
c.Id 'Customer ID', 
c.RefNumber 'RefNumber',
FullName=
CASE WHEN c.FullName is NULL
THEN 'Wizard is not finished'
ELSE c.FullName
END,  
cmp.DisplayName 'Marketplace name', 
mt.name 'Type of store', 
cmp.UpdatingStart 'Time Start', 
cmp.UpdatingEnd 'Time End',
datediff(mi, cmp.UpdatingStart, cmp.UpdatingEnd) 'Time in min',
Result=
CASE WHEN cmp.UpdateError is not NULL
THEN 'passed'
ELSE 'not passed'
end
  FROM Customer as c RIGHT join MP_CustomerMarketPlace as cmp on c.Id=cmp.CustomerId
  RIGHT join MP_MarketplaceType as mt
on cmp.MarketPlaceId=mt.Id
AND cmp.UpdatingEnd IS NOT NULL
GO
