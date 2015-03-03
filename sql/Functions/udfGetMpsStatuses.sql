
IF OBJECT_ID('dbo.udfGetMpsStatuses') IS NULL
	EXECUTE('CREATE FUNCTION dbo.udfGetMpsStatuses() RETURNS NVARCHAR(255) AS BEGIN RETURN '''' END')
GO


SET QUOTED_IDENTIFIER ON
GO

ALTER FUNCTION dbo.udfGetMpsStatuses(@CustomerId INT)
RETURNS NVARCHAR(500)
AS
BEGIN
	DECLARE @MpStatuses NVARCHAR(500) = ''

	------------------------------------------------------------------------------

	SET @MpStatuses = (SELECT t.Name +' - ' + (CASE
			WHEN mp.UpdatingStart IS NULL THEN 'Never started'
			WHEN mp.UpdateError IS NOT NULL AND LTRIM(RTRIM(mp.UpdateError)) != '' THEN 'Error'
			WHEN mp.UpdatingStart IS NOT NULL AND mp.UpdatingEnd IS NULL THEN 'In progress'
			ELSE 'Done'
		END) +', ' AS 'data()'
	FROM MP_MarketplaceType t 
	INNER JOIN MP_CustomerMarketPlace mp ON mp.MarketPlaceId = t.Id
	WHERE mp.CustomerId=@CustomerId
	FOR XML PATH(''))


	------------------------------------------------------------------------------
	RETURN @MpStatuses
END

GO
