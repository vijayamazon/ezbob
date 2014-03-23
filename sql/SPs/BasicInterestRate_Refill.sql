IF OBJECT_ID('BasicInterestRate_Refill') IS NULL
	EXECUTE('CREATE PROCEDURE BasicInterestRate_Refill AS SELECT 1')
GO

ALTER PROCEDURE BasicInterestRate_Refill
@TheList IntIntDecimalList READONLY
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM BasicInterestRate
	
	INSERT INTO 
		BasicInterestRate (FromScore, ToScore, LoanInterestBase)
	SELECT Value1, Value2, Value3 FROM @TheList
END
GO
