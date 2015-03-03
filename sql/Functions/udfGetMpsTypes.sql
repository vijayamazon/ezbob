IF OBJECT_ID('dbo.udfGetMpsTypes') IS NULL
	EXECUTE('CREATE FUNCTION dbo.udfGetMpsTypes() RETURNS NVARCHAR(255) AS BEGIN RETURN '''' END')
GO


SET QUOTED_IDENTIFIER ON
GO

ALTER FUNCTION dbo.udfGetMpsTypes(@CustomerId INT)
RETURNS NVARCHAR(255)
AS
BEGIN
	DECLARE @MpTypes NVARCHAR(255) = ''

	------------------------------------------------------------------------------

	SET @MpTypes = (SELECT CAST(count(*) AS NVARCHAR(3)) + ' ' + t.Name + ', ' AS 'data()'
	FROM MP_MarketplaceType t INNER JOIN MP_CustomerMarketPlace mp ON mp.MarketPlaceId = t.Id
	WHERE mp.CustomerId=@CustomerId
	GROUP BY t.Name
	FOR XML PATH(''))


	------------------------------------------------------------------------------
	RETURN @MpTypes
END

GO