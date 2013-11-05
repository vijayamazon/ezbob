IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_IsCustomerMarketPlacesUpdated]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_IsCustomerMarketPlacesUpdated]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_IsCustomerMarketPlacesUpdated]
(
	@pCustomerId  int,
	@isUpdated int output
)
AS
BEGIN
	DECLARE @notUpdatedCount int;
	SELECT @notUpdatedCount = COUNT(*)
	FROM Customer c, MP_CustomerMarketPlace cmp				
	WHERE 
		c.Id = @pCustomerId AND 
		cmp.CustomerId = c.Id AND 
		(cmp.UpdatingStart IS NULL OR 
		 (cmp.UpdatingStart IS NOT NULL	AND 
		  cmp.UpdatingEnd IS NULL))
	
	IF @notUpdatedCount > 0
		SET @isUpdated = 0
	ELSE
		SET @isUpdated = 1
	
	
	return @isUpdated;
	
END
GO
