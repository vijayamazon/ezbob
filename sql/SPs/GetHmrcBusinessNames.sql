IF OBJECT_ID('GetHmrcBusinessNames') IS NULL
	EXECUTE('CREATE PROCEDURE GetHmrcBusinessNames AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetHmrcBusinessNames
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT
		b.Name AS BusinessName,
		ISNULL(b.BelongsToCustomer, 0) AS BelongsToCustomer
	FROM
		MP_CustomerMarketPlace mp
		INNER JOIN MP_VatReturnRecords r ON r.CustomerMarketPlaceId = mp.Id 
		INNER JOIN Business b ON b.Id = r.BusinessId
	WHERE
		mp.CustomerId = @CustomerId
		AND
		mp.Disabled = 0
		AND
		ISNULL(r.IsDeleted, 0) = 0
END 
GO
