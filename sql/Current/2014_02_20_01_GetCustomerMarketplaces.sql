IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCustomerMarketplaces]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCustomerMarketplaces]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCustomerMarketplaces] 
	(@CustomerId INT)
AS
BEGIN
	-- Setting the isolation level to avoid deadlocks while 'waiting' in the main strategy
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	
	SELECT
		Id
	FROM
		MP_CustomerMarketplace
	WHERE
		CustomerId = @CustomerId	
END
GO
