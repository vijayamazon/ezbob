IF OBJECT_ID('dbo.udfLoadHmrcBusinessNames') IS NOT NULL
	DROP FUNCTION dbo.udfLoadHmrcBusinessNames
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfLoadHmrcBusinessNames(@CustomerID INT, @Now DATETIME)
RETURNS @output TABLE (
	RowType NVARCHAR(20) NOT NULL,
	Name NVARCHAR(100) NOT NULL,
	BusinessID INT NOT NULL,
	BelongsToCustomer BIT NULL
)
AS
BEGIN
	INSERT INTO @output(RowType, Name, BusinessID, BelongsToCustomer)
	SELECT DISTINCT
		RowType = 'HmrcBusinessName',
		b.Name,
		BusinessID = b.Id,
		b.BelongsToCustomer
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

	RETURN
END
GO
