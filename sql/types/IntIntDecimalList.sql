IF TYPE_ID('IntIntDecimalList') IS NULL
	CREATE TYPE IntIntDecimalList AS TABLE (
		Value1 INT NULL,
		Value2 INT NULL,
		Value3 DECIMAL(18, 6) NULL
	)
GO


