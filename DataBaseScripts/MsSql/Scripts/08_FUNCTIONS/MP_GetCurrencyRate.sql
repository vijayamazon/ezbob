IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetCurrencyRate]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[MP_GetCurrencyRate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Vitaly Kotov
-- Create date: 03.05.2012
-- Description:	Return Currency rate
-- =============================================
CREATE FUNCTION [dbo].[MP_GetCurrencyRate]
(
	@cCurrencyName nvarchar(50),
	@dDate datetime
)
RETURNS DECIMAL(18,8)
AS
BEGIN

    IF @cCurrencyName IS NULL
       RETURN 1
       
	DECLARE @ret decimal(18,8)	
	
	SELECT @ret = h.Price
	FROM MP_CurrencyRateHistory h
		LEFT JOIN MP_Currency c on h.CurrencyId = c.Id
	WHERE c.Name LIKE @cCurrencyName
	and h.Updated = 
	(
		SELECT MAX(h1.Updated)
		FROM MP_CurrencyRateHistory h1
		WHERE h1.CurrencyId = c.Id
			AND h1.Updated <= @dDate
	); 
	
	IF @ret IS NULL
	  SELECT @ret = c.Price
	  FROM MP_Currency c 
	  WHERE c.Name LIKE @cCurrencyName 
	  	
	 RETURN @ret

END
GO
