IF OBJECT_ID('RptLoanStats_PaypalTotalIn') IS NOT NULL
	DROP PROCEDURE RptLoanStats_PaypalTotalIn
GO

CREATE PROCEDURE RptLoanStats_PaypalTotalIn
AS
SELECT
	c.Id AS CustomerID,
	v.CustomerMarketPlaceId,
	v.Updated,
	v.ValueFloat
FROM
	MP_AnalyisisFunctionValues v
	INNER JOIN MP_CustomerMarketPlace m ON v.CustomerMarketPlaceId = m.Id
	INNER JOIN MP_MarketplaceType mt ON m.MarketPlaceId = mt.Id AND mt.InternalId = '3FA5E327-FCFD-483B-BA5A-DC1815747A28'
	INNER JOIN Customer c ON m.CustomerId = c.Id AND c.IsTest = 0
	INNER JOIN MP_AnalyisisFunction f ON v.AnalyisisFunctionId = f.Id AND f.InternalId = '9370A525-890D-402B-9BAA-5C89E9905CA2'
	INNER JOIN MP_AnalysisFunctionTimePeriod t ON v.AnalysisFunctionTimePeriodId = t.Id AND t.InternalId = '1F9E6CEF-7251-4E1C-AC35-801265E732CD'
ORDER BY
	c.Id,
	v.Updated,
	v.CustomerMarketPlaceId
GO
