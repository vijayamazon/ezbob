IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetInventoryItemsCount]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_GetInventoryItemsCount]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE MP_GetInventoryItemsCount
	@iCustomerMarketPlaceId int
AS
BEGIN
	
	DECLARE @iCountItems int;
	--
	select @iCountItems = SUM(i.Quantity)
	from MP_EbayAmazonInventory v
	left join MP_EbayAmazonInventoryItem i on i.InventoryId = v.Id
	where v.CustomerMarketPlaceId = @iCustomerMarketPlaceId
	and v.Created = (select MAX(v1.Created)
	                 from MP_EbayAmazonInventory v1
	                 where v1.CustomerMarketPlaceId = @iCustomerMarketPlaceId)
	                 
	return @iCountItems;
	
END
GO
