IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_IsCustomerMarketPlacesUpdated]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_IsCustomerMarketPlacesUpdated]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		KOT
-- Create date: 12 April 2012
-- Description:	Return MarketPlaceUpdatedState {0 - false, 1 - true} 
-- =============================================
CREATE PROCEDURE [dbo].[MP_IsCustomerMarketPlacesUpdated]
(
	@pCustomerId  int,
	@isUpdated int output
)
AS
BEGIN

	DECLARE @notUpdatedCount int;
    -- Insert statements for procedure here
	SELECT @notUpdatedCount = COUNT(*)
	FROM Customer c
		LEFT JOIN MP_CustomerMarketPlace cmp ON cmp.CustomerId = c.Id
				
	WHERE c.Id = @pCustomerId
	
		AND (cmp.UpdatingStart IS NULL
		      OR ( cmp.UpdatingStart IS NOT NULL
					AND cmp.UpdatingEnd IS NULL 
				  )
			)
	
	IF @notUpdatedCount > 0
		SET @isUpdated = 0
	ELSE
		SET @isUpdated = 1
	
	
	return @isUpdated;
	
END
GO
