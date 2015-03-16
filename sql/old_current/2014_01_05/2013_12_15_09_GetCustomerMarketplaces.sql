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
	SELECT 
		Id
	FROM
		MP_CustomerMarketPlace
	WHERE
		CustomerId = @CustomerId
END
GO
