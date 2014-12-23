IF OBJECT_ID('dbo.udfGetDefaultCurrencyID') IS NOT NULL
	DROP FUNCTION dbo.udfGetDefaultCurrencyID
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfGetDefaultCurrencyID()
RETURNS INT
AS
BEGIN
	DECLARE @ID INT 
	
	SELECT TOP 1
		@ID = Id
	FROM
		MP_Currency
	WHERE
		Name = 'GBP'

	RETURN @ID
END
GO
