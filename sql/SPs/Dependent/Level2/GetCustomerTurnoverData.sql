IF OBJECT_ID('GetCustomerTurnoverData') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerTurnoverData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomerTurnoverData
@OnlineOnly BIT,
@CustomerID INT,
@MonthCount INT,
@DateTo DATETIME = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @MpID INT
	DECLARE @MpType UNIQUEIDENTIFIER

	------------------------------------------------------------------------------

	DECLARE @PayPoint     UNIQUEIDENTIFIER = 'FC8F2710-AEDA-481D-86FF-539DD1FB76E0'
	DECLARE @CompanyFiles UNIQUEIDENTIFIER = '1C077670-6D6C-4CE9-BEBC-C1F9A9723908'

	------------------------------------------------------------------------------

	DECLARE curMp CURSOR FOR
	SELECT
		cmp.Id,
		mt.InternalId
	FROM
		MP_CustomerMarketPlace cmp
		INNER JOIN MP_MarketplaceType mt ON cmp.MarketPlaceId = mt.Id
	WHERE
		cmp.CustomerId = @CustomerID
		AND
		ISNULL(cmp.Disabled, 0) = 0
		AND
		mt.InternalId NOT IN (@PayPoint, @CompanyFiles)

	------------------------------------------------------------------------------

	OPEN curMp

	------------------------------------------------------------------------------

	FETCH NEXT FROM curMp INTO @MpID, @MpType

	------------------------------------------------------------------------------

	WHILE @@FETCH_STATUS = 0
	BEGIN

		FETCH NEXT FROM curMp INTO @MpID, @MpType
	END

	------------------------------------------------------------------------------

	CLOSE curMp

	------------------------------------------------------------------------------

	DEALLOCATE curMp
END
GO
