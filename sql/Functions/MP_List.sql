IF OBJECT_ID (N'dbo.MP_List') IS NOT NULL
	DROP FUNCTION dbo.MP_List
GO

CREATE FUNCTION [dbo].[MP_List]
(	@CustId int
)
RETURNS NVARCHAR (max)
AS
BEGIN
	DECLARE @res NVARCHAR(max)
	SET @res=(
SELECT (cast(a.counter AS varchar(255)) + ' '+mmt.Name + ',') from
(
SELECT marketPlaceId, count(marketPlaceId) AS 'Counter' FROM MP_CustomerMarketPlace
WHERE CustomerId=@CustId
group BY marketPlaceId
) a, MP_MarketplaceType mmt
WHERE a.marketPlaceId=mmt.Id
FOR xml PATH('')
)
RETURN @res	
END


GO

