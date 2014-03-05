IF OBJECT_ID (N'dbo.MP_GetCurrencyRate1') IS NOT NULL
	DROP FUNCTION dbo.MP_GetCurrencyRate1
GO

CREATE FUNCTION [dbo].[MP_GetCurrencyRate1]
(	@cCurrencyName nvarchar(50),
	@dDate datetime
)
RETURNS @retTable TABLE
(
   Rate DECIMAL(18,8)
)
AS
BEGIN

	DECLARE @ret decimal(18,8)	

    IF @cCurrencyName IS NULL
       SET @ret = 1
    ELSE
      BEGIN
			
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
	  	END
	  	
	 INSERT INTO @retTable(Rate)
		VALUES (@ret)  	
	 RETURN;

END

GO

