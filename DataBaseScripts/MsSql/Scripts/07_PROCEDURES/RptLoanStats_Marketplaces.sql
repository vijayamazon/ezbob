IF OBJECT_ID('RptLoanStats_Marketplaces') IS NOT NULL
	DROP PROCEDURE RptLoanStats_Marketplaces
GO

CREATE PROCEDURE RptLoanStats_Marketplaces
AS
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
GO
