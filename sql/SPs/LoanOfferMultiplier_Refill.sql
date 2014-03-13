IF OBJECT_ID('LoanOfferMultiplier_Refill') IS NULL
	EXECUTE('CREATE PROCEDURE LoanOfferMultiplier_Refill AS SELECT 1')
GO

ALTER PROCEDURE LoanOfferMultiplier_Refill
@TheList IntIntDecimalList READONLY
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM LoanOfferMultiplier
	
	INSERT INTO 
		LoanOfferMultiplier (StartScore, EndScore, Multiplier)
	SELECT Value1, Value2, Value3 FROM @TheList
END
GO
