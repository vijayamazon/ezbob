IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[udfCustomerMarketPlaces]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[udfCustomerMarketPlaces]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION udfCustomerMarketPlaces(@CustomerID INT)
RETURNS NVARCHAR(4000)
AS
BEGIN
	DECLARE @out NVARCHAR(4000)

	SET @out = ''

	SELECT
		@out = @out +
			(CASE @out WHEN '' THEN '' ELSE ', ' END) +
			(CASE COUNT(DISTINCT m.Id) WHEN 1 THEN '' ELSE CONVERT(NVARCHAR, COUNT(DISTINCT m.Id)) + ' ' END) +
			t.Name
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
	WHERE
		m.CustomerId = @CustomerID
	GROUP BY
		t.Name
	ORDER BY
		t.Name

	RETURN @out
END
GO
