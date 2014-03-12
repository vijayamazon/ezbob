IF TYPE_ID('IntIntDecimalList') IS NULL
	CREATE TYPE IntIntDecimalList AS TABLE (Value1 INT NULL, Value2 INT NULL, Value3 DECIMAL(18, 6) NULL)
GO

IF OBJECT_ID('TestIntIntDecimalListType') IS NULL
	EXECUTE('CREATE PROCEDURE TestIntIntDecimalListType AS SELECT 1')
GO

ALTER PROCEDURE TestIntIntDecimalListType
@TheList IntIntDecimalList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Value1, Value2, Value3 FROM @TheList
END
GO
