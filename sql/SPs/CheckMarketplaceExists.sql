SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CheckMarketplaceExists') IS NULL
	EXECUTE('CREATE PROCEDURE CheckMarketplaceExists AS SELECT 1')
GO

ALTER PROCEDURE CheckMarketplaceExists
@MpTypeID UNIQUEIDENTIFIER,
@OriginID INT,
@Token NVARCHAR(512)
AS
BEGIN
	DECLARE @AlreadyExists BIT = (CASE
		WHEN EXISTS (
			SELECT
				m.Id
			FROM
				MP_CustomerMarketPlace m
				INNER JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id AND t.InternalId = @MpTypeID
				INNER JOIN Customer c ON m.CustomerId = c.Id AND c.OriginID = @OriginID
			WHERE
				m.DisplayName = @Token
		)
			THEN 1
		ELSE 0
	END)

	SELECT AlreadyExists = @AlreadyExists
END
GO
