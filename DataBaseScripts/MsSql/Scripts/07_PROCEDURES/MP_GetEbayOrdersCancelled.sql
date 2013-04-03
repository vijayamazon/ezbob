IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetEbayOrdersCancelled]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_GetEbayOrdersCancelled]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE MP_GetEbayOrdersCancelled
	@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int
AS
BEGIN
	
	DECLARE @iCountItems int;
	--
	select @iCountItems = COUNT(i.Id)
	from MP_EbayOrderItem i
	left join MP_EbayOrder o on i.OrderId = o.Id
	where i.OrderStatus LIKE 'Cancelled' 
	   AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
	   AND DATEADD(MONTH, @iMonthsShiftNumber, i.CreatedTime ) >= GETDATE()
	                 
	return @iCountItems;

END
GO
