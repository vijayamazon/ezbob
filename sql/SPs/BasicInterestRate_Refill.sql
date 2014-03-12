IF TYPE_ID('IntIntDecimalList') IS NULL
	CREATE TYPE IntIntDecimalList AS TABLE (Value1 INT NULL, Value2 INT NULL, Value3 DECIMAL(18, 6) NULL)
GO

IF OBJECT_ID('TestIntIntDecimalListType') IS NULL
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
