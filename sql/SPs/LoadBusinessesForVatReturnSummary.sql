IF OBJECT_ID('LoadBusinessesForVatReturnSummary') IS NULL
	EXECUTE('CREATE PROCEDURE LoadBusinessesForVatReturnSummary AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadBusinessesForVatReturnSummary
@CustomerMarketplaceID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT
		RegistrationNo,
		BusinessID
	FROM
		MP_VatReturnRecords
	WHERE
		CustomerMarketplaceID = @CustomerMarketplaceID
		AND
		ISNULL(IsDeleted, 0) = 0
END
GO
