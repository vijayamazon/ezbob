IF OBJECT_ID('LoadMarketplacesLastUpdateTime') IS NULL
	EXECUTE('CREATE PROCEDURE LoadMarketplacesLastUpdateTime AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadMarketplacesLastUpdateTime
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	-- Setting the isolation level to avoid deadlocks while 'waiting' in the main strategy
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	SELECT
		MpID = m.Id,
		m.UpdatingEnd
	FROM
		MP_CustomerMarketplace m
	WHERE
		m.CustomerId = @CustomerID
		AND
		m.UpdatingEnd IS NOT NULL
END
GO
