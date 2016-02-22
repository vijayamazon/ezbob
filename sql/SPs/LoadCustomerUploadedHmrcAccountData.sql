IF OBJECT_ID('LoadCustomerUploadedHmrcAccountData') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerUploadedHmrcAccountData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadCustomerUploadedHmrcAccountData
@CustomerID INT
AS
BEGIN
	DECLARE @HMRC UNIQUEIDENTIFIER = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'

	SELECT
		MpID = m.Id,
		m.SecurityData,
		NewEmail = c.Name
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id AND t.InternalId = @HMRC
		INNER JOIN Customer c ON m.CustomerId = c.Id
	WHERE
		CustomerID = @CustomerID
		AND
		m.DisplayName LIKE '%@%'
END
GO
