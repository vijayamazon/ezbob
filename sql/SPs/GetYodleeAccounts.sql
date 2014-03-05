IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetYodleeAccounts]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetYodleeAccounts]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetYodleeAccounts]
AS
BEGIN
	SELECT Id, Username, Password 
		FROM YodleeAccounts y
		--WHERE Id > 100 AND y.CustomerId NOT IN (SELECT CustomerId FROM MP_CustomerMarketPlace WHERE MarketPlaceId=8)
END
GO
