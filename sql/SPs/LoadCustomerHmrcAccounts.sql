IF OBJECT_ID('LoadCustomerHmrcAccounts') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerHmrcAccounts AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadCustomerHmrcAccounts
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		m.Id
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType mt ON m.MarketPlaceId = mt.Id AND mt.InternalId = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'
	WHERE
		m.CustomerId = @CustomerID
END
GO
