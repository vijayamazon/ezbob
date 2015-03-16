IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_CustomerMarketplacesIsUpdated]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_CustomerMarketplacesIsUpdated]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_CustomerMarketplacesIsUpdated]
	(@CustomerId INT) 
AS
BEGIN
	DECLARE @notUpdatedCount INT
	SELECT 
		@notUpdatedCount = COUNT(1)
	FROM 
		Customer c, 
		MP_CustomerMarketPlace cmp				
	WHERE 
		c.Id = @CustomerId AND 
		cmp.CustomerId = c.Id AND 
		(cmp.UpdatingStart IS NULL OR 
		 (cmp.UpdatingStart IS NOT NULL	AND 
		  cmp.UpdatingEnd IS NULL))
	
	IF @notUpdatedCount > 0
		SELECT CAST (1 AS BIT) AS IsUpdated
	ELSE
		SELECT CAST (0 AS BIT) AS IsUpdated
END
GO
