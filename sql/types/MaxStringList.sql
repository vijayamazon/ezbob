SET QUOTED_IDENTIFIER ON
GO

IF TYPE_ID('MaxStringList') IS NOT NULL
	DROP TYPE MaxStringList
GO

CREATE TYPE MaxStringList AS TABLE (
	Value NVARCHAR(MAX) NULL
)
GO
