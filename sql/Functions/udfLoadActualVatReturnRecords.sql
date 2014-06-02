IF OBJECT_ID('dbo.udfLoadActualVatReturnRecords') IS NOT NULL
	DROP FUNCTION dbo.udfLoadActualVatReturnRecords
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfLoadActualVatReturnRecords(@CustomerMarketPlaceID INT)
RETURNS @out TABLE (RecordID INT)
AS
BEGIN
	INSERT INTO @out (RecordID)
	SELECT
		MAX(r.Id) AS RecordID
	FROM
		MP_VatReturnRecords r
	WHERE
		r.CustomerMarketPlaceId = @CustomerMarketplaceID
		AND
		ISNULL(r.IsDeleted, 0) = 0
	GROUP BY
		CONVERT(DATE, r.DateFrom),
		CONVERT(DATE, r.DateTo),
		r.RegistrationNo

	RETURN
END
GO
