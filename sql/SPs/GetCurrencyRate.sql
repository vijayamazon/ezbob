SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetCurrencyRate') IS NULL
	EXECUTE('CREATE PROCEDURE GetCurrencyRate AS SELECT 1')
GO

-- Returns currency rate of currency "@CurrencyName" relative to GBP on @SomeDate.
-- Returned rate should be used as follows:
--
-- DECLARE @Known_amount_in_USD = 10,000
-- DECLARE @Amount_in_GBP_to_find
-- DECLARE @Rate DECIMAL(18, 8)
-- EXECUTE GetCurrencyRate @SomeDate, 'USD', @Rate OUTPUT
-- SET @Amount_in_GBP_to_find = @Known_amount_in_USD * @Rate

ALTER PROCEDURE GetCurrencyRate
@TheDate DATETIME,
@CurrencyName NVARCHAR(10),
@Rate DECIMAL(18, 8) OUTPUT
AS
BEGIN
	SET @Rate = dbo.udfGetCurrencyRate(@TheDate, @CurrencyName)
END
GO
