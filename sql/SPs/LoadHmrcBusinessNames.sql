IF OBJECT_ID('LoadHmrcBusinessNames') IS NULL
	EXECUTE('CREATE PROCEDURE LoadHmrcBusinessNames AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadHmrcBusinessNames
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT
		RowType = 'HmrcBusinessName',
		b.Name,
		BusinessID = b.Id,
		BelongsToCustomer = ISNULL(b.BelongsToCustomer, 0)
	FROM
		Business b
		INNER JOIN MP_VatReturnRecords o
			ON b.Id = o.BusinessId
			AND (@Now IS NULL OR o.Created < @Now)
			AND (
				ISNULL(o.IsDeleted, 0) = 0
				OR
				(@Now IS NOT NULL AND NOT EXISTS (
					SELECT h.HistoryItemID
					FROM MP_VatReturnRecordDeleteHistory h
					WHERE h.DeletedRecordID = o.Id
					AND h.DeletedTime < @Now
				))
			)
		INNER JOIN MP_CustomerMarketPlace m
			ON o.CustomerMarketPlaceId = m.Id
			AND m.CustomerId = @CustomerID
			AND ISNULL(m.Disabled, 0) = 0
			AND (@Now IS NULL OR m.Created < @Now)
END
GO
