SET QUOTED_IDENTIFIER ON
GO

IF TYPE_ID('IntTextList') IS NULL
	CREATE TYPE IntTextList AS TABLE (
		ID INT,
		Value NTEXT
	)
GO
