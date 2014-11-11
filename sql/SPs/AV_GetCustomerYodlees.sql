SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_GetCustomerYodlees') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetCustomerYodlees AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[AV_GetCustomerYodlees] 
	(@CustomerId INT)
AS
BEGIN
	SELECT mp.Id Id, mp.DisplayName Name, t.Name Type, mp.OriginationDate FROM MP_CustomerMarketPlace mp 
	INNER JOIN MP_MarketplaceType t 
	ON t.Id = mp.MarketPlaceId 
	WHERE CustomerId=@CustomerId 
	AND mp.Disabled = 0 
	AND (t.InternalId='107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF')
END


GO


