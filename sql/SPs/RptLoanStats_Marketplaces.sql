IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptLoanStats_Marketplaces]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptLoanStats_Marketplaces]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptLoanStats_Marketplaces]
AS
BEGIN
	SELECT
	m.Id AS MarketPlaceID,
	c.Id AS CustomerID,
	t.Id AS MarketPlaceTypeID,
	t.Name AS MarketPlaceType,
	m.Created
FROM
	MP_CustomerMarketPlace m
	INNER JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
	INNER JOIN Customer c ON m.CustomerId = c.Id AND c.IsTest = 0
ORDER BY
	c.Id,
	t.Id,
	m.Created
END
GO
