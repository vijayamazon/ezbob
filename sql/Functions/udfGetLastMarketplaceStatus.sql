IF OBJECT_ID('dbo.udfGetLastMarketplaceStatus') IS NULL
	EXECUTE('CREATE FUNCTION dbo.udfGetLastMarketplaceStatus() RETURNS NVARCHAR(255) AS BEGIN RETURN '''' END')
GO

ALTER FUNCTION dbo.udfGetLastMarketplaceStatus(@MarketplaceId INT)
RETURNS NVARCHAR(255)
AS
BEGIN
	DECLARE 		
		@UpdStart DATETIME,
		@UpdEnd DATETIME,
		@CurrentStatus NVARCHAR(255),
		@HasError BIT

	------------------------------------------------------------------------------

	SELECT
		@UpdStart = UpdatingStart,
		@UpdEnd = UpdatingEnd,
		@HasError = CASE WHEN UpdateError IS NULL THEN 1 ELSE 0 END
	FROM
		MP_CustomerMarketPlace
	WHERE
		Id = @MarketplaceID

	------------------------------------------------------------------------------

	IF @HasError = 1
		SET @CurrentStatus = 'Error'
	ELSE IF @UpdEnd IS NOT NULL
		SET @CurrentStatus = 'Done'
	ELSE IF @UpdStart IS NOT NULL
		SET @CurrentStatus = 'In progress'
	ELSE
		SET @CurrentStatus = 'Never Started'

	RETURN @CurrentStatus
END
GO
